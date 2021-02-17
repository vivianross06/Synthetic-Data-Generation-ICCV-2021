using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySUNCG
{
    public class TransformUtils
    {
        // Get children list for iterating, avoid changing the iterator
        public static List<Transform> GetChildrenList(Transform transform)
        {
            int childCount = transform.childCount;
            List<Transform> children = new List<Transform>();
            foreach (Transform child in transform)
            {
                children.Add(child);
            }
            return children;
        }

        // 16*1 vector to 4*4 Matrix, column major
        public static Matrix4x4 Array2Matrix4x4(float[] transform)
        {
            Vector4 column1 = new Vector4(transform[0], transform[1], transform[2], transform[3]);
            Vector4 column2 = new Vector4(transform[4], transform[5], transform[6], transform[7]);
            Vector4 column3 = new Vector4(transform[8], transform[9], transform[10], transform[11]);
            Vector4 column4 = new Vector4(transform[12], transform[13], transform[14], transform[15]);
            return new Matrix4x4(column1, column2, column3, column4);
        }

        public static Vector3 ExtractTranslationFromMatrix(ref Matrix4x4 matrix)
        {
            Vector3 translate;
            translate.x = matrix.m03;
            translate.y = matrix.m13;
            translate.z = matrix.m23;
            return translate;
        }

        public static Quaternion ExtractRotationFromMatrix(ref Matrix4x4 matrix)
        {
            Vector3 forward;
            forward.x = matrix.m02;
            forward.y = matrix.m12;
            forward.z = matrix.m22;

            Vector3 upwards;
            upwards.x = matrix.m01;
            upwards.y = matrix.m11;
            upwards.z = matrix.m21;

            return Quaternion.LookRotation(forward, upwards);
        }


        public static Vector3 ExtractScaleFromMatrix(ref Matrix4x4 matrix)
        {
            Vector3 scale;
            scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
            scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
            scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
            return scale;
        }

        public static void DecomposeMatrix(ref Matrix4x4 matrix, out Vector3 localPosition, out Quaternion localRotation, out Vector3 localScale)
        {
            localPosition = ExtractTranslationFromMatrix(ref matrix);
            localRotation = ExtractRotationFromMatrix(ref matrix);
            localScale = ExtractScaleFromMatrix(ref matrix);
        }

        public static void SetTransformFromMatrix(Transform transform, ref Matrix4x4 matrix, bool flip = false)
        {
            if (flip)
                matrix = FlipHandedness(matrix);
            transform.localPosition = ExtractTranslationFromMatrix(ref matrix);
            transform.localRotation = ExtractRotationFromMatrix(ref matrix);
            transform.localScale = ExtractScaleFromMatrix(ref matrix);
        }

        public static readonly Matrix4x4 FLIP_Z = Matrix4x4.Scale(new Vector3(1, 1, -1));

        static public Matrix4x4 FlipHandedness(Matrix4x4 matrix)
        {
            return FLIP_Z * matrix * FLIP_Z;
        }

        private Vector3 FlipHandedness(Vector3 vector)
        {
            return new Vector3(vector.x, vector.z, vector.y);
        }

    }

}
