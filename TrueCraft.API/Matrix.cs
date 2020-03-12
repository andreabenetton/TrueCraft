// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace TrueCraft.API
{
    /// <summary>
    ///     Represents the right-handed 4x4 doubleing point matrix, which can store translation, scale and rotation
    ///     information.
    /// </summary>
    public struct Matrix : IEquatable<Matrix>
    {
        #region Public Constructors

        /// <summary>
        ///     Constructs a matrix.
        /// </summary>
        /// <param name="m11">A first row and first column value.</param>
        /// <param name="m12">A first row and second column value.</param>
        /// <param name="m13">A first row and third column value.</param>
        /// <param name="m14">A first row and fourth column value.</param>
        /// <param name="m21">A second row and first column value.</param>
        /// <param name="m22">A second row and second column value.</param>
        /// <param name="m23">A second row and third column value.</param>
        /// <param name="m24">A second row and fourth column value.</param>
        /// <param name="m31">A third row and first column value.</param>
        /// <param name="m32">A third row and second column value.</param>
        /// <param name="m33">A third row and third column value.</param>
        /// <param name="m34">A third row and fourth column value.</param>
        /// <param name="m41">A fourth row and first column value.</param>
        /// <param name="m42">A fourth row and second column value.</param>
        /// <param name="m43">A fourth row and third column value.</param>
        /// <param name="m44">A fourth row and fourth column value.</param>
        public Matrix(double m11, double m12, double m13, double m14, double m21, double m22, double m23, double m24,
            double m31,
            double m32, double m33, double m34, double m41, double m42, double m43, double m44)
        {
            M11 = m11;
            M12 = m12;
            M13 = m13;
            M14 = m14;
            M21 = m21;
            M22 = m22;
            M23 = m23;
            M24 = m24;
            M31 = m31;
            M32 = m32;
            M33 = m33;
            M34 = m34;
            M41 = m41;
            M42 = m42;
            M43 = m43;
            M44 = m44;
        }

        #endregion

        #region Public Fields

        /// <summary>
        ///     A first row and first column value.
        /// </summary>
        public double M11;

        /// <summary>
        ///     A first row and second column value.
        /// </summary>
        public double M12;

        /// <summary>
        ///     A first row and third column value.
        /// </summary>
        public double M13;

        /// <summary>
        ///     A first row and fourth column value.
        /// </summary>
        public double M14;

        /// <summary>
        ///     A second row and first column value.
        /// </summary>
        public double M21;

        /// <summary>
        ///     A second row and second column value.
        /// </summary>
        public double M22;

        /// <summary>
        ///     A second row and third column value.
        /// </summary>
        public double M23;

        /// <summary>
        ///     A second row and fourth column value.
        /// </summary>
        public double M24;

        /// <summary>
        ///     A third row and first column value.
        /// </summary>
        public double M31;

        /// <summary>
        ///     A third row and second column value.
        /// </summary>
        public double M32;

        /// <summary>
        ///     A third row and third column value.
        /// </summary>
        public double M33;

        /// <summary>
        ///     A third row and fourth column value.
        /// </summary>
        public double M34;

        /// <summary>
        ///     A fourth row and first column value.
        /// </summary>
        public double M41;

        /// <summary>
        ///     A fourth row and second column value.
        /// </summary>
        public double M42;

        /// <summary>
        ///     A fourth row and third column value.
        /// </summary>
        public double M43;

        /// <summary>
        ///     A fourth row and fourth column value.
        /// </summary>
        public double M44;

        #endregion

        #region Indexers

        public double this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return M11;
                    case 1: return M12;
                    case 2: return M13;
                    case 3: return M14;
                    case 4: return M21;
                    case 5: return M22;
                    case 6: return M23;
                    case 7: return M24;
                    case 8: return M31;
                    case 9: return M32;
                    case 10: return M33;
                    case 11: return M34;
                    case 12: return M41;
                    case 13: return M42;
                    case 14: return M43;
                    case 15: return M44;
                }

                throw new ArgumentOutOfRangeException();
            }

            set
            {
                switch (index)
                {
                    case 0:
                        M11 = value;
                        break;
                    case 1:
                        M12 = value;
                        break;
                    case 2:
                        M13 = value;
                        break;
                    case 3:
                        M14 = value;
                        break;
                    case 4:
                        M21 = value;
                        break;
                    case 5:
                        M22 = value;
                        break;
                    case 6:
                        M23 = value;
                        break;
                    case 7:
                        M24 = value;
                        break;
                    case 8:
                        M31 = value;
                        break;
                    case 9:
                        M32 = value;
                        break;
                    case 10:
                        M33 = value;
                        break;
                    case 11:
                        M34 = value;
                        break;
                    case 12:
                        M41 = value;
                        break;
                    case 13:
                        M42 = value;
                        break;
                    case 14:
                        M43 = value;
                        break;
                    case 15:
                        M44 = value;
                        break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }

        public double this[int row, int column]
        {
            get => this[row * 4 + column];

            set => this[row * 4 + column] = value;
        }

        #endregion

        #region Private Members

        #endregion

        #region Public Properties

        /// <summary>
        ///     The backward vector formed from the third row M31, M32, M33 elements.
        /// </summary>
        public Vector3 Backward
        {
            get => new Vector3(M31, M32, M33);
            set
            {
                M31 = value.X;
                M32 = value.Y;
                M33 = value.Z;
            }
        }

        /// <summary>
        ///     The down vector formed from the second row -M21, -M22, -M23 elements.
        /// </summary>
        public Vector3 Down
        {
            get => new Vector3(-M21, -M22, -M23);
            set
            {
                M21 = -value.X;
                M22 = -value.Y;
                M23 = -value.Z;
            }
        }

        /// <summary>
        ///     The forward vector formed from the third row -M31, -M32, -M33 elements.
        /// </summary>
        public Vector3 Forward
        {
            get => new Vector3(-M31, -M32, -M33);
            set
            {
                M31 = -value.X;
                M32 = -value.Y;
                M33 = -value.Z;
            }
        }

        /// <summary>
        ///     Returns the identity matrix.
        /// </summary>
        public static Matrix Identity { get; } = new Matrix(1f, 0f, 0f, 0f,
            0f, 1f, 0f, 0f,
            0f, 0f, 1f, 0f,
            0f, 0f, 0f, 1f);

        /// <summary>
        ///     The left vector formed from the first row -M11, -M12, -M13 elements.
        /// </summary>
        public Vector3 Left
        {
            get => new Vector3(-M11, -M12, -M13);
            set
            {
                M11 = -value.X;
                M12 = -value.Y;
                M13 = -value.Z;
            }
        }

        /// <summary>
        ///     The right vector formed from the first row M11, M12, M13 elements.
        /// </summary>
        public Vector3 Right
        {
            get => new Vector3(M11, M12, M13);
            set
            {
                M11 = value.X;
                M12 = value.Y;
                M13 = value.Z;
            }
        }

        /// <summary>
        ///     Position stored in this matrix.
        /// </summary>
        public Vector3 Translation
        {
            get => new Vector3(M41, M42, M43);
            set
            {
                M41 = value.X;
                M42 = value.Y;
                M43 = value.Z;
            }
        }

        /// <summary>
        ///     Scale stored in this matrix.
        /// </summary>
        public Vector3 Scale
        {
            get => new Vector3(M11, M22, M33);
            set
            {
                M11 = value.X;
                M22 = value.Y;
                M33 = value.Z;
            }
        }

        /// <summary>
        ///     The upper vector formed from the second row M21, M22, M23 elements.
        /// </summary>
        public Vector3 Up
        {
            get => new Vector3(M21, M22, M23);
            set
            {
                M21 = value.X;
                M22 = value.Y;
                M23 = value.Z;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Creates a new <see cref="Matrix" /> which contains sum of two matrixes.
        /// </summary>
        /// <param name="matrix1">The first matrix to add.</param>
        /// <param name="matrix2">The second matrix to add.</param>
        /// <returns>The result of the matrix addition.</returns>
        public static Matrix Add(Matrix matrix1, Matrix matrix2)
        {
            matrix1.M11 += matrix2.M11;
            matrix1.M12 += matrix2.M12;
            matrix1.M13 += matrix2.M13;
            matrix1.M14 += matrix2.M14;
            matrix1.M21 += matrix2.M21;
            matrix1.M22 += matrix2.M22;
            matrix1.M23 += matrix2.M23;
            matrix1.M24 += matrix2.M24;
            matrix1.M31 += matrix2.M31;
            matrix1.M32 += matrix2.M32;
            matrix1.M33 += matrix2.M33;
            matrix1.M34 += matrix2.M34;
            matrix1.M41 += matrix2.M41;
            matrix1.M42 += matrix2.M42;
            matrix1.M43 += matrix2.M43;
            matrix1.M44 += matrix2.M44;
            return matrix1;
        }

        /// <summary>
        ///     Creates a new <see cref="Matrix" /> which contains sum of two matrixes.
        /// </summary>
        /// <param name="matrix1">The first matrix to add.</param>
        /// <param name="matrix2">The second matrix to add.</param>
        /// <param name="result">The result of the matrix addition as an output parameter.</param>
        public static void Add(ref Matrix matrix1, ref Matrix matrix2, out Matrix result)
        {
            result.M11 = matrix1.M11 + matrix2.M11;
            result.M12 = matrix1.M12 + matrix2.M12;
            result.M13 = matrix1.M13 + matrix2.M13;
            result.M14 = matrix1.M14 + matrix2.M14;
            result.M21 = matrix1.M21 + matrix2.M21;
            result.M22 = matrix1.M22 + matrix2.M22;
            result.M23 = matrix1.M23 + matrix2.M23;
            result.M24 = matrix1.M24 + matrix2.M24;
            result.M31 = matrix1.M31 + matrix2.M31;
            result.M32 = matrix1.M32 + matrix2.M32;
            result.M33 = matrix1.M33 + matrix2.M33;
            result.M34 = matrix1.M34 + matrix2.M34;
            result.M41 = matrix1.M41 + matrix2.M41;
            result.M42 = matrix1.M42 + matrix2.M42;
            result.M43 = matrix1.M43 + matrix2.M43;
            result.M44 = matrix1.M44 + matrix2.M44;
        }

        /// <summary>
        ///     Creates a new rotation <see cref="Matrix" /> around X axis.
        /// </summary>
        /// <param name="radians">Angle in radians.</param>
        /// <returns>The rotation <see cref="Matrix" /> around X axis.</returns>
        public static Matrix CreateRotationX(double radians)
        {
            Matrix result;
            CreateRotationX(radians, out result);
            return result;
        }

        /// <summary>
        ///     Creates a new rotation <see cref="Matrix" /> around X axis.
        /// </summary>
        /// <param name="radians">Angle in radians.</param>
        /// <param name="result">The rotation <see cref="Matrix" /> around X axis as an output parameter.</param>
        public static void CreateRotationX(double radians, out Matrix result)
        {
            result = Identity;

            var val1 = Math.Cos(radians);
            var val2 = Math.Sin(radians);

            result.M22 = val1;
            result.M23 = val2;
            result.M32 = -val2;
            result.M33 = val1;
        }

        /// <summary>
        ///     Creates a new rotation <see cref="Matrix" /> around Y axis.
        /// </summary>
        /// <param name="radians">Angle in radians.</param>
        /// <returns>The rotation <see cref="Matrix" /> around Y axis.</returns>
        public static Matrix CreateRotationY(double radians)
        {
            Matrix result;
            CreateRotationY(radians, out result);
            return result;
        }

        /// <summary>
        ///     Creates a new rotation <see cref="Matrix" /> around Y axis.
        /// </summary>
        /// <param name="radians">Angle in radians.</param>
        /// <param name="result">The rotation <see cref="Matrix" /> around Y axis as an output parameter.</param>
        public static void CreateRotationY(double radians, out Matrix result)
        {
            result = Identity;

            var val1 = Math.Cos(radians);
            var val2 = Math.Sin(radians);

            result.M11 = val1;
            result.M13 = -val2;
            result.M31 = val2;
            result.M33 = val1;
        }

        /// <summary>
        ///     Creates a new rotation <see cref="Matrix" /> around Z axis.
        /// </summary>
        /// <param name="radians">Angle in radians.</param>
        /// <returns>The rotation <see cref="Matrix" /> around Z axis.</returns>
        public static Matrix CreateRotationZ(double radians)
        {
            Matrix result;
            CreateRotationZ(radians, out result);
            return result;
        }

        /// <summary>
        ///     Creates a new rotation <see cref="Matrix" /> around Z axis.
        /// </summary>
        /// <param name="radians">Angle in radians.</param>
        /// <param name="result">The rotation <see cref="Matrix" /> around Z axis as an output parameter.</param>
        public static void CreateRotationZ(double radians, out Matrix result)
        {
            result = Identity;

            var val1 = Math.Cos(radians);
            var val2 = Math.Sin(radians);

            result.M11 = val1;
            result.M12 = val2;
            result.M21 = -val2;
            result.M22 = val1;
        }

        /// <summary>
        ///     Creates a new scaling <see cref="Matrix" />.
        /// </summary>
        /// <param name="scale">Scale value for all three axises.</param>
        /// <returns>The scaling <see cref="Matrix" />.</returns>
        public static Matrix CreateScale(double scale)
        {
            Matrix result;
            CreateScale(scale, scale, scale, out result);
            return result;
        }

        /// <summary>
        ///     Creates a new scaling <see cref="Matrix" />.
        /// </summary>
        /// <param name="scale">Scale value for all three axises.</param>
        /// <param name="result">The scaling <see cref="Matrix" /> as an output parameter.</param>
        public static void CreateScale(double scale, out Matrix result)
        {
            CreateScale(scale, scale, scale, out result);
        }

        /// <summary>
        ///     Creates a new scaling <see cref="Matrix" />.
        /// </summary>
        /// <param name="xScale">Scale value for X axis.</param>
        /// <param name="yScale">Scale value for Y axis.</param>
        /// <param name="zScale">Scale value for Z axis.</param>
        /// <returns>The scaling <see cref="Matrix" />.</returns>
        public static Matrix CreateScale(double xScale, double yScale, double zScale)
        {
            Matrix result;
            CreateScale(xScale, yScale, zScale, out result);
            return result;
        }

        /// <summary>
        ///     Creates a new scaling <see cref="Matrix" />.
        /// </summary>
        /// <param name="xScale">Scale value for X axis.</param>
        /// <param name="yScale">Scale value for Y axis.</param>
        /// <param name="zScale">Scale value for Z axis.</param>
        /// <param name="result">The scaling <see cref="Matrix" /> as an output parameter.</param>
        public static void CreateScale(double xScale, double yScale, double zScale, out Matrix result)
        {
            result.M11 = xScale;
            result.M12 = 0;
            result.M13 = 0;
            result.M14 = 0;
            result.M21 = 0;
            result.M22 = yScale;
            result.M23 = 0;
            result.M24 = 0;
            result.M31 = 0;
            result.M32 = 0;
            result.M33 = zScale;
            result.M34 = 0;
            result.M41 = 0;
            result.M42 = 0;
            result.M43 = 0;
            result.M44 = 1;
        }

        /// <summary>
        ///     Creates a new scaling <see cref="Matrix" />.
        /// </summary>
        /// <param name="scales"><see cref="Vector3" /> representing x,y and z scale values.</param>
        /// <returns>The scaling <see cref="Matrix" />.</returns>
        public static Matrix CreateScale(Vector3 scales)
        {
            Matrix result;
            CreateScale(ref scales, out result);
            return result;
        }

        /// <summary>
        ///     Creates a new scaling <see cref="Matrix" />.
        /// </summary>
        /// <param name="scales"><see cref="Vector3" /> representing x,y and z scale values.</param>
        /// <param name="result">The scaling <see cref="Matrix" /> as an output parameter.</param>
        public static void CreateScale(ref Vector3 scales, out Matrix result)
        {
            result.M11 = scales.X;
            result.M12 = 0;
            result.M13 = 0;
            result.M14 = 0;
            result.M21 = 0;
            result.M22 = scales.Y;
            result.M23 = 0;
            result.M24 = 0;
            result.M31 = 0;
            result.M32 = 0;
            result.M33 = scales.Z;
            result.M34 = 0;
            result.M41 = 0;
            result.M42 = 0;
            result.M43 = 0;
            result.M44 = 1;
        }


        /// <summary>
        ///     Creates a new translation <see cref="Matrix" />.
        /// </summary>
        /// <param name="xPosition">X coordinate of translation.</param>
        /// <param name="yPosition">Y coordinate of translation.</param>
        /// <param name="zPosition">Z coordinate of translation.</param>
        /// <returns>The translation <see cref="Matrix" />.</returns>
        public static Matrix CreateTranslation(double xPosition, double yPosition, double zPosition)
        {
            Matrix result;
            CreateTranslation(xPosition, yPosition, zPosition, out result);
            return result;
        }

        /// <summary>
        ///     Creates a new translation <see cref="Matrix" />.
        /// </summary>
        /// <param name="position">X,Y and Z coordinates of translation.</param>
        /// <param name="result">The translation <see cref="Matrix" /> as an output parameter.</param>
        public static void CreateTranslation(ref Vector3 position, out Matrix result)
        {
            result.M11 = 1;
            result.M12 = 0;
            result.M13 = 0;
            result.M14 = 0;
            result.M21 = 0;
            result.M22 = 1;
            result.M23 = 0;
            result.M24 = 0;
            result.M31 = 0;
            result.M32 = 0;
            result.M33 = 1;
            result.M34 = 0;
            result.M41 = position.X;
            result.M42 = position.Y;
            result.M43 = position.Z;
            result.M44 = 1;
        }

        /// <summary>
        ///     Creates a new translation <see cref="Matrix" />.
        /// </summary>
        /// <param name="position">X,Y and Z coordinates of translation.</param>
        /// <returns>The translation <see cref="Matrix" />.</returns>
        public static Matrix CreateTranslation(Vector3 position)
        {
            Matrix result;
            CreateTranslation(ref position, out result);
            return result;
        }

        /// <summary>
        ///     Creates a new translation <see cref="Matrix" />.
        /// </summary>
        /// <param name="xPosition">X coordinate of translation.</param>
        /// <param name="yPosition">Y coordinate of translation.</param>
        /// <param name="zPosition">Z coordinate of translation.</param>
        /// <param name="result">The translation <see cref="Matrix" /> as an output parameter.</param>
        public static void CreateTranslation(double xPosition, double yPosition, double zPosition, out Matrix result)
        {
            result.M11 = 1;
            result.M12 = 0;
            result.M13 = 0;
            result.M14 = 0;
            result.M21 = 0;
            result.M22 = 1;
            result.M23 = 0;
            result.M24 = 0;
            result.M31 = 0;
            result.M32 = 0;
            result.M33 = 1;
            result.M34 = 0;
            result.M41 = xPosition;
            result.M42 = yPosition;
            result.M43 = zPosition;
            result.M44 = 1;
        }

        /// <summary>
        ///     Returns a determinant of this <see cref="Matrix" />.
        /// </summary>
        /// <returns>Determinant of this <see cref="Matrix" /></returns>
        /// <remarks>
        ///     See more about determinant here - http://en.wikipedia.org/wiki/Determinant.
        /// </remarks>
        public double Determinant()
        {
            var num22 = M11;
            var num21 = M12;
            var num20 = M13;
            var num19 = M14;
            var num12 = M21;
            var num11 = M22;
            var num10 = M23;
            var num9 = M24;
            var num8 = M31;
            var num7 = M32;
            var num6 = M33;
            var num5 = M34;
            var num4 = M41;
            var num3 = M42;
            var num2 = M43;
            var num = M44;
            var num18 = num6 * num - num5 * num2;
            var num17 = num7 * num - num5 * num3;
            var num16 = num7 * num2 - num6 * num3;
            var num15 = num8 * num - num5 * num4;
            var num14 = num8 * num2 - num6 * num4;
            var num13 = num8 * num3 - num7 * num4;
            return num22 * (num11 * num18 - num10 * num17 + num9 * num16) -
                   num21 * (num12 * num18 - num10 * num15 + num9 * num14) +
                   num20 * (num12 * num17 - num11 * num15 + num9 * num13) -
                   num19 * (num12 * num16 - num11 * num14 + num10 * num13);
        }

        /// <summary>
        ///     Divides the elements of a <see cref="Matrix" /> by the elements of another matrix.
        /// </summary>
        /// <param name="matrix1">Source <see cref="Matrix" />.</param>
        /// <param name="matrix2">Divisor <see cref="Matrix" />.</param>
        /// <returns>The result of dividing the matrix.</returns>
        public static Matrix Divide(Matrix matrix1, Matrix matrix2)
        {
            matrix1.M11 = matrix1.M11 / matrix2.M11;
            matrix1.M12 = matrix1.M12 / matrix2.M12;
            matrix1.M13 = matrix1.M13 / matrix2.M13;
            matrix1.M14 = matrix1.M14 / matrix2.M14;
            matrix1.M21 = matrix1.M21 / matrix2.M21;
            matrix1.M22 = matrix1.M22 / matrix2.M22;
            matrix1.M23 = matrix1.M23 / matrix2.M23;
            matrix1.M24 = matrix1.M24 / matrix2.M24;
            matrix1.M31 = matrix1.M31 / matrix2.M31;
            matrix1.M32 = matrix1.M32 / matrix2.M32;
            matrix1.M33 = matrix1.M33 / matrix2.M33;
            matrix1.M34 = matrix1.M34 / matrix2.M34;
            matrix1.M41 = matrix1.M41 / matrix2.M41;
            matrix1.M42 = matrix1.M42 / matrix2.M42;
            matrix1.M43 = matrix1.M43 / matrix2.M43;
            matrix1.M44 = matrix1.M44 / matrix2.M44;
            return matrix1;
        }

        /// <summary>
        ///     Divides the elements of a <see cref="Matrix" /> by the elements of another matrix.
        /// </summary>
        /// <param name="matrix1">Source <see cref="Matrix" />.</param>
        /// <param name="matrix2">Divisor <see cref="Matrix" />.</param>
        /// <param name="result">The result of dividing the matrix as an output parameter.</param>
        public static void Divide(ref Matrix matrix1, ref Matrix matrix2, out Matrix result)
        {
            result.M11 = matrix1.M11 / matrix2.M11;
            result.M12 = matrix1.M12 / matrix2.M12;
            result.M13 = matrix1.M13 / matrix2.M13;
            result.M14 = matrix1.M14 / matrix2.M14;
            result.M21 = matrix1.M21 / matrix2.M21;
            result.M22 = matrix1.M22 / matrix2.M22;
            result.M23 = matrix1.M23 / matrix2.M23;
            result.M24 = matrix1.M24 / matrix2.M24;
            result.M31 = matrix1.M31 / matrix2.M31;
            result.M32 = matrix1.M32 / matrix2.M32;
            result.M33 = matrix1.M33 / matrix2.M33;
            result.M34 = matrix1.M34 / matrix2.M34;
            result.M41 = matrix1.M41 / matrix2.M41;
            result.M42 = matrix1.M42 / matrix2.M42;
            result.M43 = matrix1.M43 / matrix2.M43;
            result.M44 = matrix1.M44 / matrix2.M44;
        }

        /// <summary>
        ///     Divides the elements of a <see cref="Matrix" /> by a scalar.
        /// </summary>
        /// <param name="matrix1">Source <see cref="Matrix" />.</param>
        /// <param name="divider">Divisor scalar.</param>
        /// <returns>The result of dividing a matrix by a scalar.</returns>
        public static Matrix Divide(Matrix matrix1, double divider)
        {
            var num = 1f / divider;
            matrix1.M11 = matrix1.M11 * num;
            matrix1.M12 = matrix1.M12 * num;
            matrix1.M13 = matrix1.M13 * num;
            matrix1.M14 = matrix1.M14 * num;
            matrix1.M21 = matrix1.M21 * num;
            matrix1.M22 = matrix1.M22 * num;
            matrix1.M23 = matrix1.M23 * num;
            matrix1.M24 = matrix1.M24 * num;
            matrix1.M31 = matrix1.M31 * num;
            matrix1.M32 = matrix1.M32 * num;
            matrix1.M33 = matrix1.M33 * num;
            matrix1.M34 = matrix1.M34 * num;
            matrix1.M41 = matrix1.M41 * num;
            matrix1.M42 = matrix1.M42 * num;
            matrix1.M43 = matrix1.M43 * num;
            matrix1.M44 = matrix1.M44 * num;
            return matrix1;
        }

        /// <summary>
        ///     Divides the elements of a <see cref="Matrix" /> by a scalar.
        /// </summary>
        /// <param name="matrix1">Source <see cref="Matrix" />.</param>
        /// <param name="divider">Divisor scalar.</param>
        /// <param name="result">The result of dividing a matrix by a scalar as an output parameter.</param>
        public static void Divide(ref Matrix matrix1, double divider, out Matrix result)
        {
            var num = 1f / divider;
            result.M11 = matrix1.M11 * num;
            result.M12 = matrix1.M12 * num;
            result.M13 = matrix1.M13 * num;
            result.M14 = matrix1.M14 * num;
            result.M21 = matrix1.M21 * num;
            result.M22 = matrix1.M22 * num;
            result.M23 = matrix1.M23 * num;
            result.M24 = matrix1.M24 * num;
            result.M31 = matrix1.M31 * num;
            result.M32 = matrix1.M32 * num;
            result.M33 = matrix1.M33 * num;
            result.M34 = matrix1.M34 * num;
            result.M41 = matrix1.M41 * num;
            result.M42 = matrix1.M42 * num;
            result.M43 = matrix1.M43 * num;
            result.M44 = matrix1.M44 * num;
        }

        /// <summary>
        ///     Compares whether current instance is equal to specified <see cref="Matrix" /> without any tolerance.
        /// </summary>
        /// <param name="other">The <see cref="Matrix" /> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public bool Equals(Matrix other)
        {
            return M11 == other.M11 && M22 == other.M22 && M33 == other.M33 && M44 == other.M44 && M12 == other.M12 &&
                   M13 == other.M13 && M14 == other.M14 && M21 == other.M21 && M23 == other.M23 && M24 == other.M24 &&
                   M31 == other.M31 && M32 == other.M32 && M34 == other.M34 && M41 == other.M41 && M42 == other.M42 &&
                   M43 == other.M43;
        }

        /// <summary>
        ///     Compares whether current instance is equal to specified <see cref="Object" /> without any tolerance.
        /// </summary>
        /// <param name="obj">The <see cref="Object" /> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public override bool Equals(object obj)
        {
            var flag = false;
            if (obj is Matrix) flag = Equals((Matrix) obj);
            return flag;
        }

        /// <summary>
        ///     Gets the hash code of this <see cref="Matrix" />.
        /// </summary>
        /// <returns>Hash code of this <see cref="Matrix" />.</returns>
        public override int GetHashCode()
        {
            return M11.GetHashCode() + M12.GetHashCode() + M13.GetHashCode() + M14.GetHashCode() + M21.GetHashCode() +
                   M22.GetHashCode() + M23.GetHashCode() + M24.GetHashCode() + M31.GetHashCode() + M32.GetHashCode() +
                   M33.GetHashCode() + M34.GetHashCode() + M41.GetHashCode() + M42.GetHashCode() + M43.GetHashCode() +
                   M44.GetHashCode();
        }

        /// <summary>
        ///     Creates a new <see cref="Matrix" /> which contains inversion of the specified matrix.
        /// </summary>
        /// <param name="matrix">Source <see cref="Matrix" />.</param>
        /// <returns>The inverted matrix.</returns>
        public static Matrix Invert(Matrix matrix)
        {
            Matrix result;
            Invert(ref matrix, out result);
            return result;
        }

        /// <summary>
        ///     Creates a new <see cref="Matrix" /> which contains inversion of the specified matrix.
        /// </summary>
        /// <param name="matrix">Source <see cref="Matrix" />.</param>
        /// <param name="result">The inverted matrix as output parameter.</param>
        public static void Invert(ref Matrix matrix, out Matrix result)
        {
            var num1 = matrix.M11;
            var num2 = matrix.M12;
            var num3 = matrix.M13;
            var num4 = matrix.M14;
            var num5 = matrix.M21;
            var num6 = matrix.M22;
            var num7 = matrix.M23;
            var num8 = matrix.M24;
            var num9 = matrix.M31;
            var num10 = matrix.M32;
            var num11 = matrix.M33;
            var num12 = matrix.M34;
            var num13 = matrix.M41;
            var num14 = matrix.M42;
            var num15 = matrix.M43;
            var num16 = matrix.M44;
            var num17 = num11 * num16 - num12 * num15;
            var num18 = num10 * num16 - num12 * num14;
            var num19 = num10 * num15 - num11 * num14;
            var num20 = num9 * num16 - num12 * num13;
            var num21 = num9 * num15 - num11 * num13;
            var num22 = num9 * num14 - num10 * num13;
            var num23 = num6 * num17 - num7 * num18 + num8 * num19;
            var num24 = -(num5 * num17 - num7 * num20 + num8 * num21);
            var num25 = num5 * num18 - num6 * num20 + num8 * num22;
            var num26 = -(num5 * num19 - num6 * num21 + num7 * num22);
            var num27 = 1.0 / (num1 * num23 + num2 * num24 + num3 * num25 + num4 * num26);

            result.M11 = num23 * num27;
            result.M21 = num24 * num27;
            result.M31 = num25 * num27;
            result.M41 = num26 * num27;
            result.M12 = -(num2 * num17 - num3 * num18 + num4 * num19) * num27;
            result.M22 = (num1 * num17 - num3 * num20 + num4 * num21) * num27;
            result.M32 = -(num1 * num18 - num2 * num20 + num4 * num22) * num27;
            result.M42 = (num1 * num19 - num2 * num21 + num3 * num22) * num27;
            var num28 = num7 * num16 - num8 * num15;
            var num29 = num6 * num16 - num8 * num14;
            var num30 = num6 * num15 - num7 * num14;
            var num31 = num5 * num16 - num8 * num13;
            var num32 = num5 * num15 - num7 * num13;
            var num33 = num5 * num14 - num6 * num13;
            result.M13 = (num2 * num28 - num3 * num29 + num4 * num30) * num27;
            result.M23 = -(num1 * num28 - num3 * num31 + num4 * num32) * num27;
            result.M33 = (num1 * num29 - num2 * num31 + num4 * num33) * num27;
            result.M43 = -(num1 * num30 - num2 * num32 + num3 * num33) * num27;
            var num34 = num7 * num12 - num8 * num11;
            var num35 = num6 * num12 - num8 * num10;
            var num36 = num6 * num11 - num7 * num10;
            var num37 = num5 * num12 - num8 * num9;
            var num38 = num5 * num11 - num7 * num9;
            var num39 = num5 * num10 - num6 * num9;
            result.M14 = -(num2 * num34 - num3 * num35 + num4 * num36) * num27;
            result.M24 = (num1 * num34 - num3 * num37 + num4 * num38) * num27;
            result.M34 = -(num1 * num35 - num2 * num37 + num4 * num39) * num27;
            result.M44 = (num1 * num36 - num2 * num38 + num3 * num39) * num27;


            /*
            
            
            ///
            // Use Laplace expansion theorem to calculate the inverse of a 4x4 matrix
            // 
            // 1. Calculate the 2x2 determinants needed the 4x4 determinant based on the 2x2 determinants 
            // 3. Create the adjugate matrix, which satisfies: A * adj(A) = det(A) * I
            // 4. Divide adjugate matrix with the determinant to find the inverse
            
            double det1, det2, det3, det4, det5, det6, det7, det8, det9, det10, det11, det12;
            double detMatrix;
            FindDeterminants(ref matrix, out detMatrix, out det1, out det2, out det3, out det4, out det5, out det6, 
                             out det7, out det8, out det9, out det10, out det11, out det12);
            
            double invDetMatrix = 1f / detMatrix;
            
            Matrix ret; // Allow for matrix and result to point to the same structure
            
            ret.M11 = (matrix.M22*det12 - matrix.M23*det11 + matrix.M24*det10) * invDetMatrix;
            ret.M12 = (-matrix.M12*det12 + matrix.M13*det11 - matrix.M14*det10) * invDetMatrix;
            ret.M13 = (matrix.M42*det6 - matrix.M43*det5 + matrix.M44*det4) * invDetMatrix;
            ret.M14 = (-matrix.M32*det6 + matrix.M33*det5 - matrix.M34*det4) * invDetMatrix;
            ret.M21 = (-matrix.M21*det12 + matrix.M23*det9 - matrix.M24*det8) * invDetMatrix;
            ret.M22 = (matrix.M11*det12 - matrix.M13*det9 + matrix.M14*det8) * invDetMatrix;
            ret.M23 = (-matrix.M41*det6 + matrix.M43*det3 - matrix.M44*det2) * invDetMatrix;
            ret.M24 = (matrix.M31*det6 - matrix.M33*det3 + matrix.M34*det2) * invDetMatrix;
            ret.M31 = (matrix.M21*det11 - matrix.M22*det9 + matrix.M24*det7) * invDetMatrix;
            ret.M32 = (-matrix.M11*det11 + matrix.M12*det9 - matrix.M14*det7) * invDetMatrix;
            ret.M33 = (matrix.M41*det5 - matrix.M42*det3 + matrix.M44*det1) * invDetMatrix;
            ret.M34 = (-matrix.M31*det5 + matrix.M32*det3 - matrix.M34*det1) * invDetMatrix;
            ret.M41 = (-matrix.M21*det10 + matrix.M22*det8 - matrix.M23*det7) * invDetMatrix;
            ret.M42 = (matrix.M11*det10 - matrix.M12*det8 + matrix.M13*det7) * invDetMatrix;
            ret.M43 = (-matrix.M41*det4 + matrix.M42*det2 - matrix.M43*det1) * invDetMatrix;
            ret.M44 = (matrix.M31*det4 - matrix.M32*det2 + matrix.M33*det1) * invDetMatrix;
            
            result = ret;
            */
        }

        /// <summary>
        ///     Creates a new <see cref="Matrix" /> that contains linear interpolation of the values in specified matrixes.
        /// </summary>
        /// <param name="matrix1">The first <see cref="Matrix" />.</param>
        /// <param name="matrix2">The second <see cref="Vector2" />.</param>
        /// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
        /// <returns>>The result of linear interpolation of the specified matrixes.</returns>
        public static Matrix Lerp(Matrix matrix1, Matrix matrix2, double amount)
        {
            matrix1.M11 = matrix1.M11 + (matrix2.M11 - matrix1.M11) * amount;
            matrix1.M12 = matrix1.M12 + (matrix2.M12 - matrix1.M12) * amount;
            matrix1.M13 = matrix1.M13 + (matrix2.M13 - matrix1.M13) * amount;
            matrix1.M14 = matrix1.M14 + (matrix2.M14 - matrix1.M14) * amount;
            matrix1.M21 = matrix1.M21 + (matrix2.M21 - matrix1.M21) * amount;
            matrix1.M22 = matrix1.M22 + (matrix2.M22 - matrix1.M22) * amount;
            matrix1.M23 = matrix1.M23 + (matrix2.M23 - matrix1.M23) * amount;
            matrix1.M24 = matrix1.M24 + (matrix2.M24 - matrix1.M24) * amount;
            matrix1.M31 = matrix1.M31 + (matrix2.M31 - matrix1.M31) * amount;
            matrix1.M32 = matrix1.M32 + (matrix2.M32 - matrix1.M32) * amount;
            matrix1.M33 = matrix1.M33 + (matrix2.M33 - matrix1.M33) * amount;
            matrix1.M34 = matrix1.M34 + (matrix2.M34 - matrix1.M34) * amount;
            matrix1.M41 = matrix1.M41 + (matrix2.M41 - matrix1.M41) * amount;
            matrix1.M42 = matrix1.M42 + (matrix2.M42 - matrix1.M42) * amount;
            matrix1.M43 = matrix1.M43 + (matrix2.M43 - matrix1.M43) * amount;
            matrix1.M44 = matrix1.M44 + (matrix2.M44 - matrix1.M44) * amount;
            return matrix1;
        }

        /// <summary>
        ///     Creates a new <see cref="Matrix" /> that contains linear interpolation of the values in specified matrixes.
        /// </summary>
        /// <param name="matrix1">The first <see cref="Matrix" />.</param>
        /// <param name="matrix2">The second <see cref="Vector2" />.</param>
        /// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
        /// <param name="result">The result of linear interpolation of the specified matrixes as an output parameter.</param>
        public static void Lerp(ref Matrix matrix1, ref Matrix matrix2, double amount, out Matrix result)
        {
            result.M11 = matrix1.M11 + (matrix2.M11 - matrix1.M11) * amount;
            result.M12 = matrix1.M12 + (matrix2.M12 - matrix1.M12) * amount;
            result.M13 = matrix1.M13 + (matrix2.M13 - matrix1.M13) * amount;
            result.M14 = matrix1.M14 + (matrix2.M14 - matrix1.M14) * amount;
            result.M21 = matrix1.M21 + (matrix2.M21 - matrix1.M21) * amount;
            result.M22 = matrix1.M22 + (matrix2.M22 - matrix1.M22) * amount;
            result.M23 = matrix1.M23 + (matrix2.M23 - matrix1.M23) * amount;
            result.M24 = matrix1.M24 + (matrix2.M24 - matrix1.M24) * amount;
            result.M31 = matrix1.M31 + (matrix2.M31 - matrix1.M31) * amount;
            result.M32 = matrix1.M32 + (matrix2.M32 - matrix1.M32) * amount;
            result.M33 = matrix1.M33 + (matrix2.M33 - matrix1.M33) * amount;
            result.M34 = matrix1.M34 + (matrix2.M34 - matrix1.M34) * amount;
            result.M41 = matrix1.M41 + (matrix2.M41 - matrix1.M41) * amount;
            result.M42 = matrix1.M42 + (matrix2.M42 - matrix1.M42) * amount;
            result.M43 = matrix1.M43 + (matrix2.M43 - matrix1.M43) * amount;
            result.M44 = matrix1.M44 + (matrix2.M44 - matrix1.M44) * amount;
        }

        /// <summary>
        ///     Creates a new <see cref="Matrix" /> that contains a multiplication of two matrix.
        /// </summary>
        /// <param name="matrix1">Source <see cref="Matrix" />.</param>
        /// <param name="matrix2">Source <see cref="Matrix" />.</param>
        /// <returns>Result of the matrix multiplication.</returns>
        public static Matrix Multiply(Matrix matrix1, Matrix matrix2)
        {
            var m11 = matrix1.M11 * matrix2.M11 + matrix1.M12 * matrix2.M21 + matrix1.M13 * matrix2.M31 +
                      matrix1.M14 * matrix2.M41;
            var m12 = matrix1.M11 * matrix2.M12 + matrix1.M12 * matrix2.M22 + matrix1.M13 * matrix2.M32 +
                      matrix1.M14 * matrix2.M42;
            var m13 = matrix1.M11 * matrix2.M13 + matrix1.M12 * matrix2.M23 + matrix1.M13 * matrix2.M33 +
                      matrix1.M14 * matrix2.M43;
            var m14 = matrix1.M11 * matrix2.M14 + matrix1.M12 * matrix2.M24 + matrix1.M13 * matrix2.M34 +
                      matrix1.M14 * matrix2.M44;
            var m21 = matrix1.M21 * matrix2.M11 + matrix1.M22 * matrix2.M21 + matrix1.M23 * matrix2.M31 +
                      matrix1.M24 * matrix2.M41;
            var m22 = matrix1.M21 * matrix2.M12 + matrix1.M22 * matrix2.M22 + matrix1.M23 * matrix2.M32 +
                      matrix1.M24 * matrix2.M42;
            var m23 = matrix1.M21 * matrix2.M13 + matrix1.M22 * matrix2.M23 + matrix1.M23 * matrix2.M33 +
                      matrix1.M24 * matrix2.M43;
            var m24 = matrix1.M21 * matrix2.M14 + matrix1.M22 * matrix2.M24 + matrix1.M23 * matrix2.M34 +
                      matrix1.M24 * matrix2.M44;
            var m31 = matrix1.M31 * matrix2.M11 + matrix1.M32 * matrix2.M21 + matrix1.M33 * matrix2.M31 +
                      matrix1.M34 * matrix2.M41;
            var m32 = matrix1.M31 * matrix2.M12 + matrix1.M32 * matrix2.M22 + matrix1.M33 * matrix2.M32 +
                      matrix1.M34 * matrix2.M42;
            var m33 = matrix1.M31 * matrix2.M13 + matrix1.M32 * matrix2.M23 + matrix1.M33 * matrix2.M33 +
                      matrix1.M34 * matrix2.M43;
            var m34 = matrix1.M31 * matrix2.M14 + matrix1.M32 * matrix2.M24 + matrix1.M33 * matrix2.M34 +
                      matrix1.M34 * matrix2.M44;
            var m41 = matrix1.M41 * matrix2.M11 + matrix1.M42 * matrix2.M21 + matrix1.M43 * matrix2.M31 +
                      matrix1.M44 * matrix2.M41;
            var m42 = matrix1.M41 * matrix2.M12 + matrix1.M42 * matrix2.M22 + matrix1.M43 * matrix2.M32 +
                      matrix1.M44 * matrix2.M42;
            var m43 = matrix1.M41 * matrix2.M13 + matrix1.M42 * matrix2.M23 + matrix1.M43 * matrix2.M33 +
                      matrix1.M44 * matrix2.M43;
            var m44 = matrix1.M41 * matrix2.M14 + matrix1.M42 * matrix2.M24 + matrix1.M43 * matrix2.M34 +
                      matrix1.M44 * matrix2.M44;
            matrix1.M11 = m11;
            matrix1.M12 = m12;
            matrix1.M13 = m13;
            matrix1.M14 = m14;
            matrix1.M21 = m21;
            matrix1.M22 = m22;
            matrix1.M23 = m23;
            matrix1.M24 = m24;
            matrix1.M31 = m31;
            matrix1.M32 = m32;
            matrix1.M33 = m33;
            matrix1.M34 = m34;
            matrix1.M41 = m41;
            matrix1.M42 = m42;
            matrix1.M43 = m43;
            matrix1.M44 = m44;
            return matrix1;
        }

        /// <summary>
        ///     Creates a new <see cref="Matrix" /> that contains a multiplication of two matrix.
        /// </summary>
        /// <param name="matrix1">Source <see cref="Matrix" />.</param>
        /// <param name="matrix2">Source <see cref="Matrix" />.</param>
        /// <param name="result">Result of the matrix multiplication as an output parameter.</param>
        public static void Multiply(ref Matrix matrix1, ref Matrix matrix2, out Matrix result)
        {
            var m11 = matrix1.M11 * matrix2.M11 + matrix1.M12 * matrix2.M21 + matrix1.M13 * matrix2.M31 +
                      matrix1.M14 * matrix2.M41;
            var m12 = matrix1.M11 * matrix2.M12 + matrix1.M12 * matrix2.M22 + matrix1.M13 * matrix2.M32 +
                      matrix1.M14 * matrix2.M42;
            var m13 = matrix1.M11 * matrix2.M13 + matrix1.M12 * matrix2.M23 + matrix1.M13 * matrix2.M33 +
                      matrix1.M14 * matrix2.M43;
            var m14 = matrix1.M11 * matrix2.M14 + matrix1.M12 * matrix2.M24 + matrix1.M13 * matrix2.M34 +
                      matrix1.M14 * matrix2.M44;
            var m21 = matrix1.M21 * matrix2.M11 + matrix1.M22 * matrix2.M21 + matrix1.M23 * matrix2.M31 +
                      matrix1.M24 * matrix2.M41;
            var m22 = matrix1.M21 * matrix2.M12 + matrix1.M22 * matrix2.M22 + matrix1.M23 * matrix2.M32 +
                      matrix1.M24 * matrix2.M42;
            var m23 = matrix1.M21 * matrix2.M13 + matrix1.M22 * matrix2.M23 + matrix1.M23 * matrix2.M33 +
                      matrix1.M24 * matrix2.M43;
            var m24 = matrix1.M21 * matrix2.M14 + matrix1.M22 * matrix2.M24 + matrix1.M23 * matrix2.M34 +
                      matrix1.M24 * matrix2.M44;
            var m31 = matrix1.M31 * matrix2.M11 + matrix1.M32 * matrix2.M21 + matrix1.M33 * matrix2.M31 +
                      matrix1.M34 * matrix2.M41;
            var m32 = matrix1.M31 * matrix2.M12 + matrix1.M32 * matrix2.M22 + matrix1.M33 * matrix2.M32 +
                      matrix1.M34 * matrix2.M42;
            var m33 = matrix1.M31 * matrix2.M13 + matrix1.M32 * matrix2.M23 + matrix1.M33 * matrix2.M33 +
                      matrix1.M34 * matrix2.M43;
            var m34 = matrix1.M31 * matrix2.M14 + matrix1.M32 * matrix2.M24 + matrix1.M33 * matrix2.M34 +
                      matrix1.M34 * matrix2.M44;
            var m41 = matrix1.M41 * matrix2.M11 + matrix1.M42 * matrix2.M21 + matrix1.M43 * matrix2.M31 +
                      matrix1.M44 * matrix2.M41;
            var m42 = matrix1.M41 * matrix2.M12 + matrix1.M42 * matrix2.M22 + matrix1.M43 * matrix2.M32 +
                      matrix1.M44 * matrix2.M42;
            var m43 = matrix1.M41 * matrix2.M13 + matrix1.M42 * matrix2.M23 + matrix1.M43 * matrix2.M33 +
                      matrix1.M44 * matrix2.M43;
            var m44 = matrix1.M41 * matrix2.M14 + matrix1.M42 * matrix2.M24 + matrix1.M43 * matrix2.M34 +
                      matrix1.M44 * matrix2.M44;
            result.M11 = m11;
            result.M12 = m12;
            result.M13 = m13;
            result.M14 = m14;
            result.M21 = m21;
            result.M22 = m22;
            result.M23 = m23;
            result.M24 = m24;
            result.M31 = m31;
            result.M32 = m32;
            result.M33 = m33;
            result.M34 = m34;
            result.M41 = m41;
            result.M42 = m42;
            result.M43 = m43;
            result.M44 = m44;
        }

        /// <summary>
        ///     Creates a new <see cref="Matrix" /> that contains a multiplication of <see cref="Matrix" /> and a scalar.
        /// </summary>
        /// <param name="matrix1">Source <see cref="Matrix" />.</param>
        /// <param name="scaleFactor">Scalar value.</param>
        /// <returns>Result of the matrix multiplication with a scalar.</returns>
        public static Matrix Multiply(Matrix matrix1, double scaleFactor)
        {
            matrix1.M11 *= scaleFactor;
            matrix1.M12 *= scaleFactor;
            matrix1.M13 *= scaleFactor;
            matrix1.M14 *= scaleFactor;
            matrix1.M21 *= scaleFactor;
            matrix1.M22 *= scaleFactor;
            matrix1.M23 *= scaleFactor;
            matrix1.M24 *= scaleFactor;
            matrix1.M31 *= scaleFactor;
            matrix1.M32 *= scaleFactor;
            matrix1.M33 *= scaleFactor;
            matrix1.M34 *= scaleFactor;
            matrix1.M41 *= scaleFactor;
            matrix1.M42 *= scaleFactor;
            matrix1.M43 *= scaleFactor;
            matrix1.M44 *= scaleFactor;
            return matrix1;
        }

        /// <summary>
        ///     Creates a new <see cref="Matrix" /> that contains a multiplication of <see cref="Matrix" /> and a scalar.
        /// </summary>
        /// <param name="matrix1">Source <see cref="Matrix" />.</param>
        /// <param name="scaleFactor">Scalar value.</param>
        /// <param name="result">Result of the matrix multiplication with a scalar as an output parameter.</param>
        public static void Multiply(ref Matrix matrix1, double scaleFactor, out Matrix result)
        {
            result.M11 = matrix1.M11 * scaleFactor;
            result.M12 = matrix1.M12 * scaleFactor;
            result.M13 = matrix1.M13 * scaleFactor;
            result.M14 = matrix1.M14 * scaleFactor;
            result.M21 = matrix1.M21 * scaleFactor;
            result.M22 = matrix1.M22 * scaleFactor;
            result.M23 = matrix1.M23 * scaleFactor;
            result.M24 = matrix1.M24 * scaleFactor;
            result.M31 = matrix1.M31 * scaleFactor;
            result.M32 = matrix1.M32 * scaleFactor;
            result.M33 = matrix1.M33 * scaleFactor;
            result.M34 = matrix1.M34 * scaleFactor;
            result.M41 = matrix1.M41 * scaleFactor;
            result.M42 = matrix1.M42 * scaleFactor;
            result.M43 = matrix1.M43 * scaleFactor;
            result.M44 = matrix1.M44 * scaleFactor;
        }

        /// <summary>
        ///     Copy the values of specified <see cref="Matrix" /> to the double array.
        /// </summary>
        /// <param name="matrix">The source <see cref="Matrix" />.</param>
        /// <returns>The array which matrix values will be stored.</returns>
        /// <remarks>
        ///     Required for OpenGL 2.0 projection matrix stuff.
        /// </remarks>
        public static double[] TodoubleArray(Matrix matrix)
        {
            double[] matarray =
            {
                matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                matrix.M41, matrix.M42, matrix.M43, matrix.M44
            };
            return matarray;
        }

        /// <summary>
        ///     Returns a matrix with the all values negated.
        /// </summary>
        /// <param name="matrix">Source <see cref="Matrix" />.</param>
        /// <returns>Result of the matrix negation.</returns>
        public static Matrix Negate(Matrix matrix)
        {
            matrix.M11 = -matrix.M11;
            matrix.M12 = -matrix.M12;
            matrix.M13 = -matrix.M13;
            matrix.M14 = -matrix.M14;
            matrix.M21 = -matrix.M21;
            matrix.M22 = -matrix.M22;
            matrix.M23 = -matrix.M23;
            matrix.M24 = -matrix.M24;
            matrix.M31 = -matrix.M31;
            matrix.M32 = -matrix.M32;
            matrix.M33 = -matrix.M33;
            matrix.M34 = -matrix.M34;
            matrix.M41 = -matrix.M41;
            matrix.M42 = -matrix.M42;
            matrix.M43 = -matrix.M43;
            matrix.M44 = -matrix.M44;
            return matrix;
        }

        /// <summary>
        ///     Returns a matrix with the all values negated.
        /// </summary>
        /// <param name="matrix">Source <see cref="Matrix" />.</param>
        /// <param name="result">Result of the matrix negation as an output parameter.</param>
        public static void Negate(ref Matrix matrix, out Matrix result)
        {
            result.M11 = -matrix.M11;
            result.M12 = -matrix.M12;
            result.M13 = -matrix.M13;
            result.M14 = -matrix.M14;
            result.M21 = -matrix.M21;
            result.M22 = -matrix.M22;
            result.M23 = -matrix.M23;
            result.M24 = -matrix.M24;
            result.M31 = -matrix.M31;
            result.M32 = -matrix.M32;
            result.M33 = -matrix.M33;
            result.M34 = -matrix.M34;
            result.M41 = -matrix.M41;
            result.M42 = -matrix.M42;
            result.M43 = -matrix.M43;
            result.M44 = -matrix.M44;
        }

        /// <summary>
        ///     Adds two matrixes.
        /// </summary>
        /// <param name="matrix1">Source <see cref="Matrix" /> on the left of the add sign.</param>
        /// <param name="matrix2">Source <see cref="Matrix" /> on the right of the add sign.</param>
        /// <returns>Sum of the matrixes.</returns>
        public static Matrix operator +(Matrix matrix1, Matrix matrix2)
        {
            matrix1.M11 = matrix1.M11 + matrix2.M11;
            matrix1.M12 = matrix1.M12 + matrix2.M12;
            matrix1.M13 = matrix1.M13 + matrix2.M13;
            matrix1.M14 = matrix1.M14 + matrix2.M14;
            matrix1.M21 = matrix1.M21 + matrix2.M21;
            matrix1.M22 = matrix1.M22 + matrix2.M22;
            matrix1.M23 = matrix1.M23 + matrix2.M23;
            matrix1.M24 = matrix1.M24 + matrix2.M24;
            matrix1.M31 = matrix1.M31 + matrix2.M31;
            matrix1.M32 = matrix1.M32 + matrix2.M32;
            matrix1.M33 = matrix1.M33 + matrix2.M33;
            matrix1.M34 = matrix1.M34 + matrix2.M34;
            matrix1.M41 = matrix1.M41 + matrix2.M41;
            matrix1.M42 = matrix1.M42 + matrix2.M42;
            matrix1.M43 = matrix1.M43 + matrix2.M43;
            matrix1.M44 = matrix1.M44 + matrix2.M44;
            return matrix1;
        }

        /// <summary>
        ///     Divides the elements of a <see cref="Matrix" /> by the elements of another <see cref="Matrix" />.
        /// </summary>
        /// <param name="matrix1">Source <see cref="Matrix" /> on the left of the div sign.</param>
        /// <param name="matrix2">Divisor <see cref="Matrix" /> on the right of the div sign.</param>
        /// <returns>The result of dividing the matrixes.</returns>
        public static Matrix operator /(Matrix matrix1, Matrix matrix2)
        {
            matrix1.M11 = matrix1.M11 / matrix2.M11;
            matrix1.M12 = matrix1.M12 / matrix2.M12;
            matrix1.M13 = matrix1.M13 / matrix2.M13;
            matrix1.M14 = matrix1.M14 / matrix2.M14;
            matrix1.M21 = matrix1.M21 / matrix2.M21;
            matrix1.M22 = matrix1.M22 / matrix2.M22;
            matrix1.M23 = matrix1.M23 / matrix2.M23;
            matrix1.M24 = matrix1.M24 / matrix2.M24;
            matrix1.M31 = matrix1.M31 / matrix2.M31;
            matrix1.M32 = matrix1.M32 / matrix2.M32;
            matrix1.M33 = matrix1.M33 / matrix2.M33;
            matrix1.M34 = matrix1.M34 / matrix2.M34;
            matrix1.M41 = matrix1.M41 / matrix2.M41;
            matrix1.M42 = matrix1.M42 / matrix2.M42;
            matrix1.M43 = matrix1.M43 / matrix2.M43;
            matrix1.M44 = matrix1.M44 / matrix2.M44;
            return matrix1;
        }

        /// <summary>
        ///     Divides the elements of a <see cref="Matrix" /> by a scalar.
        /// </summary>
        /// <param name="matrix">Source <see cref="Matrix" /> on the left of the div sign.</param>
        /// <param name="divider">Divisor scalar on the right of the div sign.</param>
        /// <returns>The result of dividing a matrix by a scalar.</returns>
        public static Matrix operator /(Matrix matrix, double divider)
        {
            var num = 1f / divider;
            matrix.M11 = matrix.M11 * num;
            matrix.M12 = matrix.M12 * num;
            matrix.M13 = matrix.M13 * num;
            matrix.M14 = matrix.M14 * num;
            matrix.M21 = matrix.M21 * num;
            matrix.M22 = matrix.M22 * num;
            matrix.M23 = matrix.M23 * num;
            matrix.M24 = matrix.M24 * num;
            matrix.M31 = matrix.M31 * num;
            matrix.M32 = matrix.M32 * num;
            matrix.M33 = matrix.M33 * num;
            matrix.M34 = matrix.M34 * num;
            matrix.M41 = matrix.M41 * num;
            matrix.M42 = matrix.M42 * num;
            matrix.M43 = matrix.M43 * num;
            matrix.M44 = matrix.M44 * num;
            return matrix;
        }

        /// <summary>
        ///     Compares whether two <see cref="Matrix" /> instances are equal without any tolerance.
        /// </summary>
        /// <param name="matrix1">Source <see cref="Matrix" /> on the left of the equal sign.</param>
        /// <param name="matrix2">Source <see cref="Matrix" /> on the right of the equal sign.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public static bool operator ==(Matrix matrix1, Matrix matrix2)
        {
            return matrix1.M11 == matrix2.M11 &&
                   matrix1.M12 == matrix2.M12 &&
                   matrix1.M13 == matrix2.M13 &&
                   matrix1.M14 == matrix2.M14 &&
                   matrix1.M21 == matrix2.M21 &&
                   matrix1.M22 == matrix2.M22 &&
                   matrix1.M23 == matrix2.M23 &&
                   matrix1.M24 == matrix2.M24 &&
                   matrix1.M31 == matrix2.M31 &&
                   matrix1.M32 == matrix2.M32 &&
                   matrix1.M33 == matrix2.M33 &&
                   matrix1.M34 == matrix2.M34 &&
                   matrix1.M41 == matrix2.M41 &&
                   matrix1.M42 == matrix2.M42 &&
                   matrix1.M43 == matrix2.M43 &&
                   matrix1.M44 == matrix2.M44;
        }

        /// <summary>
        ///     Compares whether two <see cref="Matrix" /> instances are not equal without any tolerance.
        /// </summary>
        /// <param name="matrix1">Source <see cref="Matrix" /> on the left of the not equal sign.</param>
        /// <param name="matrix2">Source <see cref="Matrix" /> on the right of the not equal sign.</param>
        /// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
        public static bool operator !=(Matrix matrix1, Matrix matrix2)
        {
            return matrix1.M11 != matrix2.M11 ||
                   matrix1.M12 != matrix2.M12 ||
                   matrix1.M13 != matrix2.M13 ||
                   matrix1.M14 != matrix2.M14 ||
                   matrix1.M21 != matrix2.M21 ||
                   matrix1.M22 != matrix2.M22 ||
                   matrix1.M23 != matrix2.M23 ||
                   matrix1.M24 != matrix2.M24 ||
                   matrix1.M31 != matrix2.M31 ||
                   matrix1.M32 != matrix2.M32 ||
                   matrix1.M33 != matrix2.M33 ||
                   matrix1.M34 != matrix2.M34 ||
                   matrix1.M41 != matrix2.M41 ||
                   matrix1.M42 != matrix2.M42 ||
                   matrix1.M43 != matrix2.M43 ||
                   matrix1.M44 != matrix2.M44;
        }

        /// <summary>
        ///     Multiplies two matrixes.
        /// </summary>
        /// <param name="matrix1">Source <see cref="Matrix" /> on the left of the mul sign.</param>
        /// <param name="matrix2">Source <see cref="Matrix" /> on the right of the mul sign.</param>
        /// <returns>Result of the matrix multiplication.</returns>
        /// <remarks>
        ///     Using matrix multiplication algorithm - see http://en.wikipedia.org/wiki/Matrix_multiplication.
        /// </remarks>
        public static Matrix operator *(Matrix matrix1, Matrix matrix2)
        {
            var m11 = matrix1.M11 * matrix2.M11 + matrix1.M12 * matrix2.M21 + matrix1.M13 * matrix2.M31 +
                      matrix1.M14 * matrix2.M41;
            var m12 = matrix1.M11 * matrix2.M12 + matrix1.M12 * matrix2.M22 + matrix1.M13 * matrix2.M32 +
                      matrix1.M14 * matrix2.M42;
            var m13 = matrix1.M11 * matrix2.M13 + matrix1.M12 * matrix2.M23 + matrix1.M13 * matrix2.M33 +
                      matrix1.M14 * matrix2.M43;
            var m14 = matrix1.M11 * matrix2.M14 + matrix1.M12 * matrix2.M24 + matrix1.M13 * matrix2.M34 +
                      matrix1.M14 * matrix2.M44;
            var m21 = matrix1.M21 * matrix2.M11 + matrix1.M22 * matrix2.M21 + matrix1.M23 * matrix2.M31 +
                      matrix1.M24 * matrix2.M41;
            var m22 = matrix1.M21 * matrix2.M12 + matrix1.M22 * matrix2.M22 + matrix1.M23 * matrix2.M32 +
                      matrix1.M24 * matrix2.M42;
            var m23 = matrix1.M21 * matrix2.M13 + matrix1.M22 * matrix2.M23 + matrix1.M23 * matrix2.M33 +
                      matrix1.M24 * matrix2.M43;
            var m24 = matrix1.M21 * matrix2.M14 + matrix1.M22 * matrix2.M24 + matrix1.M23 * matrix2.M34 +
                      matrix1.M24 * matrix2.M44;
            var m31 = matrix1.M31 * matrix2.M11 + matrix1.M32 * matrix2.M21 + matrix1.M33 * matrix2.M31 +
                      matrix1.M34 * matrix2.M41;
            var m32 = matrix1.M31 * matrix2.M12 + matrix1.M32 * matrix2.M22 + matrix1.M33 * matrix2.M32 +
                      matrix1.M34 * matrix2.M42;
            var m33 = matrix1.M31 * matrix2.M13 + matrix1.M32 * matrix2.M23 + matrix1.M33 * matrix2.M33 +
                      matrix1.M34 * matrix2.M43;
            var m34 = matrix1.M31 * matrix2.M14 + matrix1.M32 * matrix2.M24 + matrix1.M33 * matrix2.M34 +
                      matrix1.M34 * matrix2.M44;
            var m41 = matrix1.M41 * matrix2.M11 + matrix1.M42 * matrix2.M21 + matrix1.M43 * matrix2.M31 +
                      matrix1.M44 * matrix2.M41;
            var m42 = matrix1.M41 * matrix2.M12 + matrix1.M42 * matrix2.M22 + matrix1.M43 * matrix2.M32 +
                      matrix1.M44 * matrix2.M42;
            var m43 = matrix1.M41 * matrix2.M13 + matrix1.M42 * matrix2.M23 + matrix1.M43 * matrix2.M33 +
                      matrix1.M44 * matrix2.M43;
            var m44 = matrix1.M41 * matrix2.M14 + matrix1.M42 * matrix2.M24 + matrix1.M43 * matrix2.M34 +
                      matrix1.M44 * matrix2.M44;
            matrix1.M11 = m11;
            matrix1.M12 = m12;
            matrix1.M13 = m13;
            matrix1.M14 = m14;
            matrix1.M21 = m21;
            matrix1.M22 = m22;
            matrix1.M23 = m23;
            matrix1.M24 = m24;
            matrix1.M31 = m31;
            matrix1.M32 = m32;
            matrix1.M33 = m33;
            matrix1.M34 = m34;
            matrix1.M41 = m41;
            matrix1.M42 = m42;
            matrix1.M43 = m43;
            matrix1.M44 = m44;
            return matrix1;
        }

        /// <summary>
        ///     Multiplies the elements of matrix by a scalar.
        /// </summary>
        /// <param name="matrix">Source <see cref="Matrix" /> on the left of the mul sign.</param>
        /// <param name="scaleFactor">Scalar value on the right of the mul sign.</param>
        /// <returns>Result of the matrix multiplication with a scalar.</returns>
        public static Matrix operator *(Matrix matrix, double scaleFactor)
        {
            matrix.M11 = matrix.M11 * scaleFactor;
            matrix.M12 = matrix.M12 * scaleFactor;
            matrix.M13 = matrix.M13 * scaleFactor;
            matrix.M14 = matrix.M14 * scaleFactor;
            matrix.M21 = matrix.M21 * scaleFactor;
            matrix.M22 = matrix.M22 * scaleFactor;
            matrix.M23 = matrix.M23 * scaleFactor;
            matrix.M24 = matrix.M24 * scaleFactor;
            matrix.M31 = matrix.M31 * scaleFactor;
            matrix.M32 = matrix.M32 * scaleFactor;
            matrix.M33 = matrix.M33 * scaleFactor;
            matrix.M34 = matrix.M34 * scaleFactor;
            matrix.M41 = matrix.M41 * scaleFactor;
            matrix.M42 = matrix.M42 * scaleFactor;
            matrix.M43 = matrix.M43 * scaleFactor;
            matrix.M44 = matrix.M44 * scaleFactor;
            return matrix;
        }

        /// <summary>
        ///     Subtracts the values of one <see cref="Matrix" /> from another <see cref="Matrix" />.
        /// </summary>
        /// <param name="matrix1">Source <see cref="Matrix" /> on the left of the sub sign.</param>
        /// <param name="matrix2">Source <see cref="Matrix" /> on the right of the sub sign.</param>
        /// <returns>Result of the matrix subtraction.</returns>
        public static Matrix operator -(Matrix matrix1, Matrix matrix2)
        {
            matrix1.M11 = matrix1.M11 - matrix2.M11;
            matrix1.M12 = matrix1.M12 - matrix2.M12;
            matrix1.M13 = matrix1.M13 - matrix2.M13;
            matrix1.M14 = matrix1.M14 - matrix2.M14;
            matrix1.M21 = matrix1.M21 - matrix2.M21;
            matrix1.M22 = matrix1.M22 - matrix2.M22;
            matrix1.M23 = matrix1.M23 - matrix2.M23;
            matrix1.M24 = matrix1.M24 - matrix2.M24;
            matrix1.M31 = matrix1.M31 - matrix2.M31;
            matrix1.M32 = matrix1.M32 - matrix2.M32;
            matrix1.M33 = matrix1.M33 - matrix2.M33;
            matrix1.M34 = matrix1.M34 - matrix2.M34;
            matrix1.M41 = matrix1.M41 - matrix2.M41;
            matrix1.M42 = matrix1.M42 - matrix2.M42;
            matrix1.M43 = matrix1.M43 - matrix2.M43;
            matrix1.M44 = matrix1.M44 - matrix2.M44;
            return matrix1;
        }

        /// <summary>
        ///     Inverts values in the specified <see cref="Matrix" />.
        /// </summary>
        /// <param name="matrix">Source <see cref="Matrix" /> on the right of the sub sign.</param>
        /// <returns>Result of the inversion.</returns>
        public static Matrix operator -(Matrix matrix)
        {
            matrix.M11 = -matrix.M11;
            matrix.M12 = -matrix.M12;
            matrix.M13 = -matrix.M13;
            matrix.M14 = -matrix.M14;
            matrix.M21 = -matrix.M21;
            matrix.M22 = -matrix.M22;
            matrix.M23 = -matrix.M23;
            matrix.M24 = -matrix.M24;
            matrix.M31 = -matrix.M31;
            matrix.M32 = -matrix.M32;
            matrix.M33 = -matrix.M33;
            matrix.M34 = -matrix.M34;
            matrix.M41 = -matrix.M41;
            matrix.M42 = -matrix.M42;
            matrix.M43 = -matrix.M43;
            matrix.M44 = -matrix.M44;
            return matrix;
        }

        /// <summary>
        ///     Creates a new <see cref="Matrix" /> that contains subtraction of one matrix from another.
        /// </summary>
        /// <param name="matrix1">The first <see cref="Matrix" />.</param>
        /// <param name="matrix2">The second <see cref="Matrix" />.</param>
        /// <returns>The result of the matrix subtraction.</returns>
        public static Matrix Subtract(Matrix matrix1, Matrix matrix2)
        {
            matrix1.M11 = matrix1.M11 - matrix2.M11;
            matrix1.M12 = matrix1.M12 - matrix2.M12;
            matrix1.M13 = matrix1.M13 - matrix2.M13;
            matrix1.M14 = matrix1.M14 - matrix2.M14;
            matrix1.M21 = matrix1.M21 - matrix2.M21;
            matrix1.M22 = matrix1.M22 - matrix2.M22;
            matrix1.M23 = matrix1.M23 - matrix2.M23;
            matrix1.M24 = matrix1.M24 - matrix2.M24;
            matrix1.M31 = matrix1.M31 - matrix2.M31;
            matrix1.M32 = matrix1.M32 - matrix2.M32;
            matrix1.M33 = matrix1.M33 - matrix2.M33;
            matrix1.M34 = matrix1.M34 - matrix2.M34;
            matrix1.M41 = matrix1.M41 - matrix2.M41;
            matrix1.M42 = matrix1.M42 - matrix2.M42;
            matrix1.M43 = matrix1.M43 - matrix2.M43;
            matrix1.M44 = matrix1.M44 - matrix2.M44;
            return matrix1;
        }

        /// <summary>
        ///     Creates a new <see cref="Matrix" /> that contains subtraction of one matrix from another.
        /// </summary>
        /// <param name="matrix1">The first <see cref="Matrix" />.</param>
        /// <param name="matrix2">The second <see cref="Matrix" />.</param>
        /// <param name="result">The result of the matrix subtraction as an output parameter.</param>
        public static void Subtract(ref Matrix matrix1, ref Matrix matrix2, out Matrix result)
        {
            result.M11 = matrix1.M11 - matrix2.M11;
            result.M12 = matrix1.M12 - matrix2.M12;
            result.M13 = matrix1.M13 - matrix2.M13;
            result.M14 = matrix1.M14 - matrix2.M14;
            result.M21 = matrix1.M21 - matrix2.M21;
            result.M22 = matrix1.M22 - matrix2.M22;
            result.M23 = matrix1.M23 - matrix2.M23;
            result.M24 = matrix1.M24 - matrix2.M24;
            result.M31 = matrix1.M31 - matrix2.M31;
            result.M32 = matrix1.M32 - matrix2.M32;
            result.M33 = matrix1.M33 - matrix2.M33;
            result.M34 = matrix1.M34 - matrix2.M34;
            result.M41 = matrix1.M41 - matrix2.M41;
            result.M42 = matrix1.M42 - matrix2.M42;
            result.M43 = matrix1.M43 - matrix2.M43;
            result.M44 = matrix1.M44 - matrix2.M44;
        }

        /// <summary>
        ///     Returns a <see cref="String" /> representation of this <see cref="Matrix" /> in the format:
        ///     {M11:[<see cref="M11" />] M12:[<see cref="M12" />] M13:[<see cref="M13" />] M14:[<see cref="M14" />]}
        ///     {M21:[<see cref="M21" />] M12:[<see cref="M22" />] M13:[<see cref="M23" />] M14:[<see cref="M24" />]}
        ///     {M31:[<see cref="M31" />] M32:[<see cref="M32" />] M33:[<see cref="M33" />] M34:[<see cref="M34" />]}
        ///     {M41:[<see cref="M41" />] M42:[<see cref="M42" />] M43:[<see cref="M43" />] M44:[<see cref="M44" />]}
        /// </summary>
        /// <returns>A <see cref="String" /> representation of this <see cref="Matrix" />.</returns>
        public override string ToString()
        {
            return "{M11:" + M11 + " M12:" + M12 + " M13:" + M13 + " M14:" + M14 + "}"
                   + " {M21:" + M21 + " M22:" + M22 + " M23:" + M23 + " M24:" + M24 + "}"
                   + " {M31:" + M31 + " M32:" + M32 + " M33:" + M33 + " M34:" + M34 + "}"
                   + " {M41:" + M41 + " M42:" + M42 + " M43:" + M43 + " M44:" + M44 + "}";
        }

        /// <summary>
        ///     Swap the matrix rows and columns.
        /// </summary>
        /// <param name="matrix">The matrix for transposing operation.</param>
        /// <returns>The new <see cref="Matrix" /> which contains the transposing result.</returns>
        public static Matrix Transpose(Matrix matrix)
        {
            Matrix ret;
            Transpose(ref matrix, out ret);
            return ret;
        }

        /// <summary>
        ///     Swap the matrix rows and columns.
        /// </summary>
        /// <param name="matrix">The matrix for transposing operation.</param>
        /// <param name="result">The new <see cref="Matrix" /> which contains the transposing result as an output parameter.</param>
        public static void Transpose(ref Matrix matrix, out Matrix result)
        {
            Matrix ret;

            ret.M11 = matrix.M11;
            ret.M12 = matrix.M21;
            ret.M13 = matrix.M31;
            ret.M14 = matrix.M41;

            ret.M21 = matrix.M12;
            ret.M22 = matrix.M22;
            ret.M23 = matrix.M32;
            ret.M24 = matrix.M42;

            ret.M31 = matrix.M13;
            ret.M32 = matrix.M23;
            ret.M33 = matrix.M33;
            ret.M34 = matrix.M43;

            ret.M41 = matrix.M14;
            ret.M42 = matrix.M24;
            ret.M43 = matrix.M34;
            ret.M44 = matrix.M44;

            result = ret;
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        ///     Helper method for using the Laplace expansion theorem using two rows expansions to calculate major and
        ///     minor determinants of a 4x4 matrix. This method is used for inverting a matrix.
        /// </summary>
        private static void FindDeterminants(ref Matrix matrix, out double major,
            out double minor1, out double minor2, out double minor3, out double minor4, out double minor5,
            out double minor6,
            out double minor7, out double minor8, out double minor9, out double minor10, out double minor11,
            out double minor12)
        {
            var det1 = matrix.M11 * matrix.M22 - matrix.M12 * matrix.M21;
            var det2 = matrix.M11 * matrix.M23 - matrix.M13 * matrix.M21;
            var det3 = matrix.M11 * matrix.M24 - matrix.M14 * matrix.M21;
            var det4 = matrix.M12 * matrix.M23 - matrix.M13 * matrix.M22;
            var det5 = matrix.M12 * matrix.M24 - matrix.M14 * matrix.M22;
            var det6 = matrix.M13 * matrix.M24 - matrix.M14 * matrix.M23;
            var det7 = matrix.M31 * matrix.M42 - matrix.M32 * matrix.M41;
            var det8 = matrix.M31 * matrix.M43 - matrix.M33 * matrix.M41;
            var det9 = matrix.M31 * matrix.M44 - matrix.M34 * matrix.M41;
            var det10 = matrix.M32 * matrix.M43 - matrix.M33 * matrix.M42;
            var det11 = matrix.M32 * matrix.M44 - matrix.M34 * matrix.M42;
            var det12 = matrix.M33 * matrix.M44 - matrix.M34 * matrix.M43;

            major = det1 * det12 - det2 * det11 + det3 * det10 + det4 * det9 - det5 * det8 + det6 * det7;
            minor1 = det1;
            minor2 = det2;
            minor3 = det3;
            minor4 = det4;
            minor5 = det5;
            minor6 = det6;
            minor7 = det7;
            minor8 = det8;
            minor9 = det9;
            minor10 = det10;
            minor11 = det11;
            minor12 = det12;
        }

        #endregion
    }
}