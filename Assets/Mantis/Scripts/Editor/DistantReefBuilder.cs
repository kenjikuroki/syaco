using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class DistantReefBuilder
{
    const string Folder = "Assets/Mantis/Art/ShallowReef/";
    static T Save<T>(T asset, string name) where T:Object
    {
        var old=AssetDatabase.LoadAssetAtPath<T>(Folder+name);
        if(old) { EditorUtility.CopySerialized(asset,old); Object.DestroyImmediate(asset); return old; }
        AssetDatabase.CreateAsset(asset,Folder+name); return asset;
    }
    public static void Build(Camera camera)
    {
        var root=new GameObject("Distant ocean backdrop - flat screen");
        var vertices=new List<Vector3>(); var triangles=new List<int>();
        const int sides=128, rings=20;
        for(int r=0;r<=rings;r++) for(int a=0;a<sides;a++)
        {
            float radius=Mathf.Lerp(14,68,r/(float)rings), angle=a*Mathf.PI*2/sides;
            float x=Mathf.Cos(angle)*radius,z=Mathf.Sin(angle)*radius;
            float relief=Mathf.SmoothStep(0,1,Mathf.InverseLerp(17,31,radius));
            float height=-.12f+relief*(.5f+2.2f*Mathf.PerlinNoise(x*.065f+21,z*.065f+31));
            vertices.Add(new Vector3(x,height,z));
        }
        for(int r=0;r<rings;r++) for(int a=0;a<sides;a++)
        {
            int p=r*sides+a,q=r*sides+(a+1)%sides;
            triangles.Add(p);triangles.Add(q);triangles.Add(p+sides);
            triangles.Add(q);triangles.Add(q+sides);triangles.Add(p+sides);
        }
        var mesh=new Mesh {name="Distant seabed slopes"}; mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
        mesh=Save(mesh,"Distant seabed.asset");
        Material Material(string name,Color color) {var m=new Material(Shader.Find("Mantis/Distant Reef"));m.SetColor("_BaseColor",color);m.enableInstancing=true;return Save(m,name+".mat");}
        var ground=new GameObject("Rolling seabed");ground.transform.SetParent(root.transform,false);ground.AddComponent<MeshFilter>().sharedMesh=mesh;
        var renderer=ground.AddComponent<MeshRenderer>();renderer.sharedMaterial=Material("Distant sand",new Color(.30f,.47f,.42f));renderer.shadowCastingMode=ShadowCastingMode.Off;
        var stone=Material("Distant rock haze",new Color(.24f,.43f,.46f));
        var rng=new System.Random(732);
        for(int i=0;i<36;i++)
        {
            float angle=(float)rng.NextDouble()*Mathf.PI*2,radius=i<18?23:39;
            radius+=(float)rng.NextDouble()*13;
            string path="Assets/PolyOne/Rocks Stylized/Prefabs/SM_Rocks_"+(i%11+1).ToString("D2")+".prefab";
            var ob=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path));
            ob.name="Distant ridge "+i;ob.transform.SetParent(root.transform,false);ob.transform.rotation=Quaternion.Euler(0,i*137.5f,0);
            var rs=ob.GetComponentsInChildren<Renderer>();Bounds b=rs[0].bounds;foreach(var rr in rs)b.Encapsulate(rr.bounds);
            ob.transform.localScale*= (i<18?3.8f:6f)/Mathf.Max(b.size.x,b.size.y,b.size.z);
            var lowScale=ob.transform.localScale;lowScale.y*=.48f;ob.transform.localScale=lowScale; b=rs[0].bounds;foreach(var rr in rs)b.Encapsulate(rr.bounds);
            ob.transform.position+=new Vector3(Mathf.Sin(angle)*radius,-.3f+Mathf.SmoothStep(0,1,Mathf.InverseLerp(17,31,radius))*(.5f+2.2f*Mathf.PerlinNoise(Mathf.Sin(angle)*radius*.065f+21,Mathf.Cos(angle)*radius*.065f+31)),Mathf.Cos(angle)*radius)-new Vector3(b.center.x,b.min.y,b.center.z);
            foreach(var rr in rs) {var mats=rr.sharedMaterials;for(int m=0;m<mats.Length;m++)mats[m]=stone;rr.sharedMaterials=mats;rr.shadowCastingMode=ShadowCastingMode.Off;}
            foreach(var c in ob.GetComponentsInChildren<Collider>())Object.DestroyImmediate(c);
        }
        // Batched ribbon fronds: broad silhouettes fade with the distant reef.
        var kelpVertices=new List<Vector3>();var kelpTriangles=new List<int>();
        // Sparse, irregular beds with open water between them, independent of rock positions.
        for(int cluster=0;cluster<13;cluster++)
        {
            float angle=(float)rng.NextDouble()*Mathf.PI*2,radius=22+(float)rng.NextDouble()*19;
            float x=Mathf.Sin(angle)*radius,z=Mathf.Cos(angle)*radius;
            float floor=-.12f+Mathf.SmoothStep(0,1,Mathf.InverseLerp(17,31,radius))*(.5f+2.2f*Mathf.PerlinNoise(x*.065f+21,z*.065f+31));
            int blades=3+rng.Next(3);
            for(int blade=0;blade<blades;blade++)
            {
                float height=.65f+(float)rng.NextDouble()*1.05f,width=.075f+(float)rng.NextDouble()*.09f;
                float yaw=(float)rng.NextDouble()*Mathf.PI*2;
                Vector3 side=new Vector3(Mathf.Cos(yaw),0,Mathf.Sin(yaw));
                Vector3 origin=new Vector3(x+(float)rng.NextDouble()*1.7f-.85f,floor-.15f,z+(float)rng.NextDouble()*1.7f-.85f);
                int start=kelpVertices.Count;
                for(int row=0;row<=6;row++)
                {
                    float t=row/6f;Vector3 center=origin+Vector3.up*height*t+side*(Mathf.Sin(t*3.6f+blade)*.45f*t);
                    kelpVertices.Add(center-side*width*(1-t*.92f));kelpVertices.Add(center+side*width*(1-t*.92f));
                }
                for(int row=0;row<6;row++)
                {
                    int a=start+row*2;
                    kelpTriangles.AddRange(new[]{a,a+2,a+1,a+1,a+2,a+3,a+1,a+2,a,a+3,a+2,a+1});
                }
            }
        }
        var kelpMesh=new Mesh {name="Distant kelp beds"};kelpMesh.SetVertices(kelpVertices);kelpMesh.SetTriangles(kelpTriangles,0);kelpMesh.RecalculateNormals();kelpMesh.RecalculateBounds();
        var kelp=new GameObject("Distant seaweed beds");kelp.transform.SetParent(root.transform,false);
        kelp.AddComponent<MeshFilter>().sharedMesh=Save(kelpMesh,"Distant kelp.asset");
        var kelpRenderer=kelp.AddComponent<MeshRenderer>();kelpRenderer.sharedMaterial=Material("Distant kelp",new Color(.10f,.29f,.29f));kelpRenderer.shadowCastingMode=ShadowCastingMode.Off;
        var sky=Save(new Material(Shader.Find("Mantis/Water Sky")),"Water sky.mat");
        RenderSettings.skybox=sky;camera.clearFlags=CameraClearFlags.Skybox;camera.farClipPlane=100;
        PrefabUtility.SaveAsPrefabAssetAndConnect(root,Folder+"DistantOcean.prefab",InteractionMode.AutomatedAction);
        AssetDatabase.SaveAssets();
    }
}


