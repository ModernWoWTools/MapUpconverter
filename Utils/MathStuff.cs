using System.Numerics;
using Warcraft.NET.Files.Structures;

namespace MapUpconverter
{
    public static class MathStuff
    {
        public static float CalculateMaxRadius(BoundingBox boundingBox)
        {
            float absoluteRadius = Math.Abs(boundingBox.Maximum.X - boundingBox.Minimum.X);
            float maxRadius = absoluteRadius;

            absoluteRadius = Math.Abs(boundingBox.Maximum.Y - boundingBox.Minimum.Y);
            if (absoluteRadius > maxRadius)
            {
                maxRadius = absoluteRadius;
            }

            absoluteRadius = Math.Abs(boundingBox.Maximum.Z - boundingBox.Minimum.Z);
            if (absoluteRadius > maxRadius)
            {
                maxRadius = absoluteRadius;
            }

            return maxRadius;
        }

        public static float DegreeToRadians(float degrees)
        {
            return Convert.ToSingle(Math.PI * (double)degrees / 180.0);
        }

        public static BoundingBox CalculateBoundingBox(Vector3 position, Vector3 rotation, BoundingBox originalBox, float scale)
        {
            var serverPosition = new Vector3(17066.666f - position[0], position[1], 17066.666f - position[2]);

            Matrix4x4 modelMatrix = Matrix4x4.Identity;

            // Scale by model scale
            modelMatrix *= Matrix4x4.CreateScale(scale / 1024f);

            // Normalize rotation to a normal non-WoW system
            modelMatrix *= Matrix4x4.CreateRotationX(DegreeToRadians(rotation[2] - 90f));
            modelMatrix *= Matrix4x4.CreateRotationZ(DegreeToRadians(-rotation[0]));
            modelMatrix *= Matrix4x4.CreateRotationY(DegreeToRadians(rotation[1] - 270f));

            // Translate by server position
            modelMatrix *= Matrix4x4.CreateTranslation(serverPosition);

            // Rotate to sane orientation
            modelMatrix *= Matrix4x4.CreateRotationY(DegreeToRadians(90f));
            modelMatrix *= Matrix4x4.CreateRotationX(DegreeToRadians(90f));

            Vector3[] minMaxMatrix =
            [
                new Vector3(originalBox.Minimum.X, originalBox.Minimum.Y, originalBox.Minimum.Z),
                new Vector3(originalBox.Maximum.X, originalBox.Maximum.Y, originalBox.Maximum.Z),
                new Vector3(originalBox.Minimum.X, originalBox.Minimum.Y, originalBox.Maximum.Z),
                new Vector3(originalBox.Minimum.X, originalBox.Maximum.Y, originalBox.Minimum.Z),
                new Vector3(originalBox.Maximum.X, originalBox.Minimum.Y, originalBox.Minimum.Z),
                new Vector3(originalBox.Minimum.X, originalBox.Maximum.Y, originalBox.Maximum.Z),
                new Vector3(originalBox.Maximum.X, originalBox.Minimum.Y, originalBox.Maximum.Z),
                new Vector3(originalBox.Maximum.X, originalBox.Maximum.Y, originalBox.Minimum.Z),
            ];

            for (int i = 0; i < 8; i++)
            {
                minMaxMatrix[i] = Vector3.Transform(minMaxMatrix[i], modelMatrix);
            }

            BoundingBox newBoundingBox = new()
            {
                Minimum = new(float.MaxValue, float.MaxValue, float.MaxValue),
                Maximum = new(float.MinValue, float.MinValue, float.MinValue)
            };

            for (int j = 0; j < 8; j++)
            {
                if (minMaxMatrix[j][0] > newBoundingBox.Maximum.X)
                    newBoundingBox.Maximum.X = minMaxMatrix[j][0];

                if (minMaxMatrix[j][0] < newBoundingBox.Minimum.X)
                    newBoundingBox.Minimum.X = minMaxMatrix[j][0];

                if (minMaxMatrix[j][1] > newBoundingBox.Maximum.Y)
                    newBoundingBox.Maximum.Y = minMaxMatrix[j][1];

                if (minMaxMatrix[j][1] < newBoundingBox.Minimum.Y)
                    newBoundingBox.Minimum.Y = minMaxMatrix[j][1];

                if (minMaxMatrix[j][2] > newBoundingBox.Maximum.Z)
                    newBoundingBox.Maximum.Z = minMaxMatrix[j][2];

                if (minMaxMatrix[j][2] < newBoundingBox.Minimum.Z)
                    newBoundingBox.Minimum.Z = minMaxMatrix[j][2];
            }

            return newBoundingBox;
        }
    }
}
