using UnityEngine;

namespace Game
{
    public class Gun : MonoBehaviour
    {
        void Update()
        {
            Vector3 cursorPosition = UnityEngine.Camera.main.ScreenToWorldPoint(Input.mousePosition);
            cursorPosition.z = 0f;
            Vector3 direction = cursorPosition - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, angle));
        }
    }
}
