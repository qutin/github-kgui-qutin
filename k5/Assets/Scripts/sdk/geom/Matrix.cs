using System;
using System.Collections.Generic;
using UnityEngine;

namespace KGUI
{
    public class Matrix
    {

        public float a;
        public float b;
        public float c;
        public float d;
        public float tx;
        public float ty;
        public Matrix(float a = 1, float b = 0, float c = 0, float d = 1, float tx = 0, float ty = 0)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
            this.tx = tx;
            this.ty = ty;
        }
        public Matrix clone()
        {
            return new Matrix(a, b, c, d, tx, ty);
        }
        public void concat(Matrix m)
        {
            float a = this.a;
            float b = this.b;
            float c = this.c;
            float d = this.d;
            float tx = this.tx;
            float ty = this.ty;
            this.a = m.a * a + m.c * b;
            this.b = m.b * a + m.d * b;
            this.c = m.a * c + m.c * d;
            this.d = m.b * c + m.d * d;
            this.tx = m.a * tx + m.c * ty + m.tx;
            this.ty = m.b * tx + m.d * ty + m.ty;
        }

        public void createBox(float scaleX, float scaleY, float rotation = 0, float tx = 0, float ty = 0)
        {
            // all inlined for higher performance:
            if (rotation == 0)
            {
                a = d = 1;
                b = c = 0;
            }
            else
            {
                a = (float)Math.Cos(rotation);
                b = (float)Math.Sin(rotation);
                c = -b;
                d = a;
            }
            if (scaleX != 1)
            {
                a *= scaleX;
                c *= scaleX;
            }
            if (scaleY != 1)
            {
                b *= scaleY;
                d *= scaleY;
            }
            this.tx = tx;
            this.ty = ty;
        }
        public void createGradientBox(float width, float height, float rotation = 0, float tx = 0, float ty = 0)
        {
            this.createBox(width / MAGIC_GRADIENT_FACTOR, height / MAGIC_GRADIENT_FACTOR, rotation, tx + width / 2, ty + height / 2);
        }
        public Point deltaTransformPoint(Point point)
        {
            return new Point(a * point.x + c * point.y, b * point.x + d * point.y);
        }

        public void identity()
        {
            a = d = 1;
            b = c = tx = ty = 0;
        }
        public void invert()
        {
            float a = this.a;
            float b = this.b;
            float c = this.c;
            float d = this.d;
            float tx = this.tx;
            float ty = this.ty;
            // Cremer's rule: inverse = adjugate / determinant
            // A-1 = adj(A) / det(A)
            float det = a * d - c * b;
            //     [a11 a12 a13]
            // A = [a21 a22 a23]
            //     [a31 a32 a33]
            // according to http://de.wikipedia.org/wiki/Inverse_Matrix#Formel_f.C3.BCr_3x3-Matrizen (sorry, German):
            //          [a22*a33-a32*a23 a13*a32-a12*a33 a12*a23-a13*a22]
            // adj(A) = [a23*a31-a21*a33 a11*a33-a13*a31 a13*a21-a11*a23]
            //          [a21*a32-a22*a31 a12*a31-a11*a32 a11*a22-a12*a21]
            // with a11 = a, a12 = c, a13 = tx,
            //      a21 = b, a22 = d, a23 = ty,
            //      a31 = 0, a32 = 0, a33 = 1:
            //          [d *1-0*ty  tx*0-c *1  c *ty-tx*d ]
            // adj(A) = [ty*0-b* 1  a *1-tx*0  tx* b-a *ty]
            //          [b *0-d* 0  c *0-a *0  a * d-c *b ]
            //          [ d -c  c*ty-tx*d]
            //        = [-b  a  tx*b-a*ty]
            //          [ 0  0  a*d -c*b ]
            this.a = d / det;
            this.b = -b / det;
            this.c = -c / det;
            this.d = a / det;
            //this.tx = (c*ty-tx*d)/det;
            //this.ty = (tx*b-a*ty)/det;
            // Dart version:
            this.tx = -(this.a * tx + this.c * ty);
            this.ty = -(this.b * tx + this.d * ty);
        }
        public void rotate(float angle)
        {
            /*
                with sin = sin(angle) and cos = cos(angle):

                              [a            c            tx           ]
                              [b            d            ty           ]
                              [0            0            1            ]

              [cos   -sin  0] [a*cos-b*sin  c*cos-d*sin  tx*cos-ty*sin]
              [sin   cos   0] [a*sin+b*cos  c*sin+d*cos  tx*sin+ty*cos]
              [0     0     1] [0            0            1            ]
            */
            if (angle != 0)
            {
                float cos = (float)Math.Cos(angle);
                float sin = (float)Math.Sin(angle);
                float a = this.a;
                float b = this.b;
                float c = this.c;
                float d = this.d;
                float tx = this.tx;
                float ty = this.ty;
                this.a = a * cos - b * sin;
                this.b = a * sin + b * cos;
                this.c = c * cos - d * sin;
                this.d = c * sin + d * cos;
                this.tx = tx * cos - ty * sin;
                this.ty = tx * sin + ty * cos;
            }
        }
        public void scale(float sx, float sy)
        {
            /*
                              [a     c    tx   ]
                              [b     d    ty   ]
                              [0     0    1    ]
      
              [sx    0     0] [a*sx  c*sx tx*sx]
              [0     sy    0] [b*sy  d*sy ty*sy]
              [0     0     1] [0     0    1    ]
            */
            if (sx != 1)
            {
                a *= sx;
                c *= sx;
                tx *= sx;
            }
            if (sy != 1)
            {
                b *= sy;
                d *= sy;
                ty *= sy;
            }
        }

        public string toString()
        {
            return "(" + "[a=" + a + "b=" + b + "c=" + c + "d=" + d + "tx=" + tx + "ty=" + ty + ")";
        }

        public Point transformPoint(Point point)
        {
            return new Point(a * point.x + c * point.y + tx, b * point.x + d * point.y + ty);
        }

        public void translate(float dx, float dy)
        {
            /*
                              [a     c    tx   ]
                              [b     d    ty   ]
                              [0     0    1    ]
      
              [1     0   dx]  [a     c    tx+dx]
              [0     1   dy]  [b     d    ty+dy]
              [0     0    1]  [0     0    1    ]
            */
            tx += dx;
            ty += dy;
        }

        public void setTo(float a, float b, float c, float d, float tx, float ty)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
            this.tx = tx;
            this.ty = ty;
        }

        public void copyFrom(Matrix matrix)
        {
            this.a = matrix.a;
            this.b = matrix.b;
            this.c = matrix.c;
            this.d = matrix.d;
            this.tx = matrix.tx;
            this.ty = matrix.ty;
        }

        // ************************** Jangaroo part **************************

        public void copyFromAndConcat(Matrix copyMatrix, Matrix concatMatrix)
        {
            float a1 = copyMatrix.a;
            float b1 = copyMatrix.b;
            float c1 = copyMatrix.c;
            float d1 = copyMatrix.d;
            float tx1 = copyMatrix.tx;
            float ty1 = copyMatrix.ty;

            float a2 = concatMatrix.a;
            float b2 = concatMatrix.b;
            float c2 = concatMatrix.c;
            float d2 = concatMatrix.d;
            float tx2 = concatMatrix.tx;
            float ty2 = concatMatrix.ty;

            a = (a1 * a2 + b1 * c2);
            b = (a1 * b2 + b1 * d2);
            c = (c1 * a2 + d1 * c2);
            d = (c1 * b2 + d1 * d2);
            tx = (tx1 * a2 + ty1 * c2 + tx2);
            ty = (tx1 * b2 + ty1 * d2 + ty2);
        }

        //-------------------------------------------------------------------------------------------------

        public void copyFromAndInvert(Matrix matrix)
        {
            float a = matrix.a;
            float b = matrix.b;
            float c = matrix.c;
            float d = matrix.d;
            float tx = matrix.tx;
            float ty = matrix.ty;

            float det = a * d - b * c;
            this.a = (d / det);
            this.b = -(b / det);
            this.c = -(c / det);
            this.d = (a / det);
            this.tx = -(this.a * tx + this.c * ty);
            this.ty = -(this.b * tx + this.d * ty);
        }

        /**
         * @private
         */
        public const float MAGIC_GRADIENT_FACTOR = 16384 / 10;


        public Vector2 transformCoords(float x, float y)
        {
            Vector2 resultPoint;            
            resultPoint.x = a * x + c * y + tx;
            resultPoint.y = d * y + b * x + ty;
            
            return resultPoint;
        }

        private static float[] sHelperCoords = new float[8];
        private static Vector2 sHelperPoint =Vector2.zero;
        //public Rectangle transformRect(float x, float y, float width, float height, Rectangle resultRect)
        //{
        //    if(resultRect==null) resultRect = new Rectangle();

        //    sHelperCoords[0] = x;
        //    sHelperCoords[1] = y;
        //    sHelperCoords[2] = x+width;
        //    sHelperCoords[3] = y;
        //    sHelperCoords[4] = x+width;
        //    sHelperCoords[5] = y+height;
        //    sHelperCoords[6] = x;
        //    sHelperCoords[7] = y+height;
        //    float minX = float.MaxValue, maxX = float.MinValue;
        //    float minY = float.MaxValue, maxY = float.MinValue;

        //    for (int i = 0; i < 8; i += 2)
        //    {
        //        sHelperPoint = transformCoords(sHelperCoords[i], sHelperCoords[i + 1]);
        //        if (minX > sHelperPoint.x) minX = sHelperPoint.x;
        //        if (maxX < sHelperPoint.x) maxX = sHelperPoint.x;
        //        if (minY > sHelperPoint.y) minY = sHelperPoint.y;
        //        if (maxY < sHelperPoint.y) maxY = sHelperPoint.y;
        //    }
        //    resultRect.setTo(minX, minY, maxX - minX, maxY - minY);
        //    return resultRect;
        //}

        public Rect transformRect(float x, float y, float width, float height)
        {
            sHelperCoords[0] = x;
            sHelperCoords[1] = y;
            sHelperCoords[2] = x + width;
            sHelperCoords[3] = y;
            sHelperCoords[4] = x + width;
            sHelperCoords[5] = y + height;
            sHelperCoords[6] = x;
            sHelperCoords[7] = y + height;
            float minX = float.MaxValue, maxX = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;

            for (int i = 0; i < 8; i += 2)
            {
                sHelperPoint = transformCoords(sHelperCoords[i], sHelperCoords[i + 1]);
                if (minX > sHelperPoint.x) minX = sHelperPoint.x;
                if (maxX < sHelperPoint.x) maxX = sHelperPoint.x;
                if (minY > sHelperPoint.y) minY = sHelperPoint.y;
                if (maxY < sHelperPoint.y) maxY = sHelperPoint.y;
            }
            Rect resultRect = new Rect(minX, minY, maxX - minX, maxY - minY);
            return resultRect;
        }

        //public Rectangle transformRectNoRotate(float x, float y, float width, float height, Rectangle resultRect)
        //{
        //    if(resultRect==null) resultRect = new Rectangle();

        //    float x1,y1;
        //    sHelperPoint = transformCoords(x, y );
        //    x1 = sHelperPoint.x;
        //    y1 = sHelperPoint.y;
        //    resultRect.x = x1;
        //    resultRect.y = y1;

        //    sHelperPoint = transformCoords(x + width, y);
        //    resultRect.width = (float)Math.Sqrt(Math.Pow(sHelperPoint.x-x1,2)+Math.Pow(sHelperPoint.y-y1,2));
        //    x1 = sHelperPoint.x;
        //    y1 = sHelperPoint.y;

        //    sHelperPoint = transformCoords(x+ width, y + height);
        //    resultRect.height = (float)Math.Sqrt(Math.Pow(sHelperPoint.x - x1, 2) + Math.Pow(sHelperPoint.y - y1, 2));

        //    return resultRect;
        //}
        public Rect transformRectNoRotate(float x, float y, float width, float height)
        {
            Rect resultRect = new Rect( 0, 0, 0, 0);
            float x1, y1;
            sHelperPoint = transformCoords(x, y);
            x1 = sHelperPoint.x;
            y1 = sHelperPoint.y;
            resultRect.x = x1;
            resultRect.y = y1;

            sHelperPoint = transformCoords(x + width, y);
            resultRect.width = (float)Math.Sqrt(Math.Pow(sHelperPoint.x - x1, 2) + Math.Pow(sHelperPoint.y - y1, 2));
            x1 = sHelperPoint.x;
            y1 = sHelperPoint.y;

            sHelperPoint = transformCoords(x + width, y + height);
            resultRect.height = (float)Math.Sqrt(Math.Pow(sHelperPoint.x - x1, 2) + Math.Pow(sHelperPoint.y - y1, 2));

            return resultRect;
        }
        public void skew(float skewX, float skewY)
        {
            float sinX = (float)Math.Sin(skewX);
            float cosX = (float)Math.Cos(skewX);
            float sinY = (float)Math.Sin(skewY);
            float cosY = (float)Math.Cos(skewY);
            setTo(a  * cosY - b  * sinX,
                         a  * sinY + b  * cosX,
                         c  * cosY - d  * sinX,
                         c  * sinY + d  * cosX,
                         tx * cosY - ty * sinX,
                         tx * sinY + ty * cosX);
        }

        public void prependMatrix(Matrix prep)
        {
            setTo((float)(this.a * prep.a + this.c * prep.b),
                       (float)(this.b * prep.a + this.d * prep.b),
                      (float) (this.a * prep.c + this.c * prep.d),
                       (float)(this.b * prep.c + this.d * prep.d),
                       (float)(this.tx + this.a * prep.tx + this.c * prep.ty),
                       (float)(this.ty + this.b * prep.tx + this.d * prep.ty));
        }
    }
}

