using SharpDX;


namespace WindowsFormsApp4
{
    class FPScamera
    {
        Matrix View = Matrix.Identity;
        
        Vector3 mPosition;
        Vector3 mTarget;
        Vector3 mUp;

        int Width = 0;
        int Height = 0;

        int px = 0;
        int py = 0;

        float Zoom = 0;

        
        public FPScamera(int width, int height)
        {
            Width = width;
            Height = height;

            mPosition = new Vector3(10.0f, 0.0f, 0.0f);
            mTarget = new Vector3(0.0f, 0.0f, 0.0f);
            mUp = new Vector3(0.0f, 0.0f, 1.0f);

            View = Matrix.LookAtLH(mPosition, mTarget, mUp);
        }

        public void Set_Frame(int delta_X, int delta_Y, Vector3 Pos, int zoom, float eyes, float torsodir)
        {
            Zoom += zoom / 120.0f;

            Vector3 Vec = new Vector3(0.0f, Zoom, 0.0f);

            px += delta_X;
            py += delta_Y;

            if (Zoom < 0)
            {
                mPosition = Pos + new Vector3(0.0f, 0.0f, eyes) + Vector3.TransformCoordinate(Vec, Matrix.RotationX(-py / 1000.0f) * Matrix.RotationZ(px / 1000.0f + torsodir + 3.14159f));

                mTarget = Pos + new Vector3(0.0f, 0.0f, eyes);
            }
            else if (Zoom > 0)
            {
                mPosition = Pos + new Vector3(0.0f, 0.0f, eyes) + Vector3.TransformCoordinate(Vec, Matrix.RotationX(-py / 1000.0f) * Matrix.RotationZ(px / 1000.0f + torsodir + 3.14159f)); ;

                mTarget = mPosition + 1.05f * Vector3.TransformCoordinate(Vec, Matrix.RotationX(-py / 1000.0f) * Matrix.RotationZ(px / 1000.0f + torsodir + 3.14159f));
            }
            else
            {
                mPosition = Pos + new Vector3(0.0f, 0.0f, eyes);

                mTarget = mPosition + Vector3.TransformCoordinate(new Vector3(0.0f, 1.0f, 0.0f), Matrix.RotationX(-py / 1000.0f) * Matrix.RotationZ(px / 1000.0f + torsodir + 3.14159f));
            }

            View = Matrix.LookAtLH(mPosition, mTarget, mUp);
        }

        public Matrix Get_View_Matrix()
        {
            return View;
        }

        public Vector4 Get_CamPos()
        {
            return new Vector4(mPosition, 0.0f);
        }
    }
}