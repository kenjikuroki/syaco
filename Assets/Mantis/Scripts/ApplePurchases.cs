using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;
using Unity.Services.CloudCode;
namespace MantisPunch {
[Serializable] public sealed class AppleProductConfig {
 public string id;public bool configured=true;public ProductType type=ProductType.Consumable;public int shells;
}
[Serializable] public sealed class AppleProductCatalog {public bool paymentsEnabled;public AppleProductConfig[] products=new AppleProductConfig[0];}
[Serializable] public sealed class PurchaseGrant {public bool verified;public string transactionId;public string productId;public int shells;public bool premium;}
public sealed class ApplePurchases:MonoBehaviour {
 static ApplePurchases instance;public static ApplePurchases Instance{get{if(!instance){instance=new GameObject("Apple purchases").AddComponent<ApplePurchases>();DontDestroyOnLoad(instance.gameObject);}return instance;}}
 public AppleProductCatalog Catalog{get;private set;}=new AppleProductCatalog();int selected;public AppleProductConfig Config=>Catalog.products[selected];public void Select(int index){selected=Mathf.Clamp(index,0,Catalog.products.Length-1);product=store?.GetProducts().FirstOrDefault(p=>p.definition.id==Config.id);}
 public string Status{get;private set;}="";public bool Busy{get;private set;}public bool Ready=>store!=null&&product!=null&&product.availableToPurchase&&Catalog.paymentsEnabled&&Config.configured&&!Busy;
 public string Price=>product?.metadata.localizedPriceString??"—";
 StoreController store;Product product;bool connecting;readonly HashSet<string> processing=new HashSet<string>();
 static string T(string ja,string en)=>MantisDuel.Tr(ja,en);
 void Awake(){var json=Resources.Load<TextAsset>("AppleProduct");if(json)Catalog=JsonUtility.FromJson<AppleProductCatalog>(json.text);}
 public async void Prepare(){
  if(connecting||store!=null)return;
  if(!Catalog.paymentsEnabled){Status=T("ストアの準備中です。現在は購入できません。","Store setup is in progress. Purchases are not yet available.");return;}
  if(Application.platform!=RuntimePlatform.IPhonePlayer){Status=T("購入はiPhoneで利用できます。","Purchases are available on iPhone.");return;}
  connecting=true;
  try{await OnlineServices.SignIn();store=UnityIAPServices.StoreController();store.OnProductsFetched+=products=>{product=products.FirstOrDefault(p=>p.definition.id==Config.id);Status=product==null?T("商品を取得できません。","Product unavailable."):"";store.FetchPurchases();};
   store.OnProductsFetchFailed+=failure=>Status=T("商品を取得できません。","Product unavailable.");
   store.OnPurchasePending+=Fulfill;
   store.OnPurchaseFailed+=order=>{Busy=false;Status=T("購入は完了していません。","Purchase was not completed.");};
   store.OnPurchaseDeferred+=order=>{Busy=false;Status=T("購入の承認待ちです。","Waiting for purchase approval.");};
   store.OnPurchasesFetched+=orders=>{foreach(var pending in orders.PendingOrders)Fulfill(pending);foreach(var confirmed in orders.ConfirmedOrders)if(confirmed.CartOrdered.Items().Any(i=>i.Product.definition.type!=ProductType.Consumable))Restore(confirmed);};
   store.OnPurchasesFetchFailed+=failure=>{Busy=false;Status=T("購入履歴を取得できません。","Could not fetch purchases.");};
   store.OnStoreDisconnected+=failure=>{Busy=false;Status=T("ストアへの接続が切れました。","Store disconnected.");};
   await store.Connect();store.FetchProducts(Catalog.products.Where(p=>p.configured).Select(p=>new ProductDefinition(p.id,p.type)).ToList());
  }catch(Exception ex){Status=T("ストアに接続できません。","Store unavailable.");Debug.LogWarning("IAP initialization: "+ex.GetType().Name);store=null;}
  finally{connecting=false;}
 }
 public void Buy(){if(!Ready){Prepare();return;}Busy=true;Status=T("購入を確認中…","Confirming purchase…");store.PurchaseProduct(product);}
 public void RestorePurchases(){if(store==null){Prepare();return;}if(Busy)return;Busy=true;store.RestoreTransactions((success,message)=>{Busy=false;Status=success?T("購入履歴を確認しました。","Purchase history refreshed."):T("復元できませんでした。","Restore failed.");if(success)store.FetchPurchases();});}
 async void Fulfill(PendingOrder order){await Redeem(order,true);}
 async void Restore(Order order){await Redeem(order,false);}
 async System.Threading.Tasks.Task Redeem(Order order,bool pending){
  string transaction=order.Info.TransactionID;if(string.IsNullOrEmpty(transaction)||!processing.Add(transaction))return;
  try{
   var item=order.CartOrdered.Items().SingleOrDefault();var definition=item==null?null:Catalog.products.FirstOrDefault(p=>p.id==item.Product.definition.id);if(definition==null)return;
   await OnlineServices.SignIn();
   // The server must validate Apple-signed data, bundle/product IDs and revocation.
   // Never finish the StoreKit transaction before durable reward fulfillment.
   var grant=await CloudCodeService.Instance.CallEndpointAsync<PurchaseGrant>("ShakoRedeemApplePurchase",new Dictionary<string,object>{{"transactionId",transaction},{"productId",definition.id},{"receipt",order.Info.Receipt},{"jws",order.Info.Apple?.jwsRepresentation??""}});
   if(grant==null||!grant.verified||grant.transactionId!=transaction||grant.productId!=definition.id||grant.shells!=definition.shells||grant.premium)throw new InvalidOperationException("Unverified purchase");
   MissionStore.GrantPurchase(grant);
   if(pending)store.ConfirmPurchase((PendingOrder)order);
   Status=T("購入内容を反映しました。","Purchase applied.");
  }catch(Exception ex){Status=T("購入内容の確認待ちです。再購入せず、購入を復元してください。","Purchase verification pending. Restore purchases; do not buy again.");Debug.LogWarning("Purchase pending verification: "+ex.GetType().Name);}
  finally{processing.Remove(transaction);Busy=false;}
 }
}
}
