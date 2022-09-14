using ICities;
using UnityEngine;
using ColossalFramework;

namespace mirage_city_mod
{

    public class CameraManipulator
    {

        CameraController _controller;
        public CameraManipulator()
        {
            _controller = ToolsModifierControl.cameraController;
        }
        public Vector3 Pos
        {
            get => _controller.m_currentPosition;
            set
            {
                _controller.m_targetPosition.x = value.x;
                _controller.m_targetPosition.y = _controller.m_currentPosition.y;
                _controller.m_targetPosition.z = value.z;
            }
        }

        public float Size
        {
            get => _controller.m_currentSize;
            set => _controller.m_targetSize = value;
        }
    }


}