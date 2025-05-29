using UnityEngine;

namespace Bang
{
    public class Aim : MonoBehaviour
    {
        private void Update()
        {
            transform.position = Input.mousePosition;
        }
    }
}
