using UnityEngine;
using System.Collections;

namespace mirage_city_mod
{

    public class CamController : MonoBehaviour
    {
        static readonly WaitForSeconds interval = new WaitForSeconds(0.25f);
        CameraController controller;

        public void Start()
        {
            controller = ToolsModifierControl.cameraController;
        }

        public Scene CurrentScene()
        {
            var s = new Scene(controller.m_currentPosition, controller.m_currentSize, controller.m_currentAngle);

            return s;
        }
        public static bool Approx(Vector3 value, Vector3 other)
        {
            var dist = Vector3.Distance(value, other);
            return dist < 1.0;
        }

        public static bool Approx(Vector2 value, Vector2 other)
        {
            var dist = Vector2.Distance(value, other);
            return dist < 1.0;
        }


        public IEnumerator SetScene(Scene scene)
        {
            controller.m_targetAngle = scene.Angle();
            var pos = new Vector3(
                scene.x,
                controller.m_currentPosition.y,
                scene.z);
            controller.m_targetPosition = pos;
            controller.m_targetSize = scene.size;

            while (
                !((Approx(controller.m_targetAngle, controller.m_currentAngle)) &&
                (Approx(controller.m_targetPosition, controller.m_currentPosition))))
            {
                yield return interval;
            }
            yield return null;
        }


        public void ShowPos()
        {
            Debug.Log(CurrentScene().ToString());
        }

    }



}