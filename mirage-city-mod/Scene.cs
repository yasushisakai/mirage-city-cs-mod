namespace mirage_city_mod
{

    using UnityEngine;
    public class Scene
    {
        public float x;
        public float z;
        public float size;
        public float yaw;
        public float pitch;

        // Set Camera to Origin facing downwards, north up
        public static Scene Origin()
        {
            var settings = Properties.Settings.Default;
            var scene = new Scene(
                settings.camOriginX, 
                settings.camOriginZ, 
                settings.camOriginSize, 
                settings.camOriginAngleX, 
                settings.camOriginAngleY);
            return scene;
        }

        public Scene(float _x, float _z, float _size, float _yaw, float _pitch)
        {
            x = _x;
            z = _z;
            size = _size;
            yaw = _yaw;
            pitch = _pitch;
        }

        public Scene(Vector3 pos, float _size, Vector2 angle)
        {
            x = pos.x;
            z = pos.z;
            size = _size;
            yaw = angle.x;
            pitch = angle.y;
        }

        public override string ToString()
        {
            return $"(({x}, {z}), size: {size}, yaw: {yaw}, pitch: {pitch})";
        }

        public Vector2 Angle()
        {
            return new Vector2(yaw, pitch);
        }

    }
}