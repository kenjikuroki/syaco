using UnityEngine;
namespace MantisPunch
{
    public sealed class MantisCamera : MonoBehaviour
    {
        public MantisPlayer player;
        [Range(3, 10)] public float distance = 6.5f;
        Vector3 velocity;
        void Start() { transform.position = Desired; }
        Vector3 Desired => player.transform.position + new Vector3(0, distance * .72f, -distance);
        void LateUpdate()
        {
            transform.position = Vector3.SmoothDamp(transform.position, Desired, ref velocity, .12f);
            Vector3 target = player.transform.position + Vector3.up * .4f;
            transform.LookAt(target);
        }
    }
}
