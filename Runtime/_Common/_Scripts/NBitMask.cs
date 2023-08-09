using System;
using Unity.Burst;

namespace Nextension
{
    [BurstCompile]
    public struct NBitMask128 : IComparable<NBitMask128>, IEquatable<NBitMask128>
    {
        public long mask0;
        public long mask1;
        public bool isOnly1() => mask0 == -1 && mask1 == -1;
        public bool isOnly0() => mask0 == 0 && mask1 == 0;
        public void setBit1(int bitIndex)
        {
            if (bitIndex < 64)
            {
                mask0 = NUtils.setBit1(mask0, bitIndex);
            }
            else
            {
                mask1 = NUtils.setBit1(mask1, bitIndex % 64);
            }
        }
        public void setBit0(int bitIndex)
        {
            if (bitIndex < 64)
            {
                mask0 = NUtils.setBit0(mask0, bitIndex);
            }
            else
            {
                mask1 = NUtils.setBit0(mask1, bitIndex % 64);
            }
        }

        public int CompareTo(NBitMask128 other)
        {
            if (mask0 != other.mask0)
            {
                return mask0.CompareTo(other.mask0);
            }
            return mask1.CompareTo(other.mask1);
        }

        public bool Equals(NBitMask128 other)
        {
            return CompareTo(other) == 0;
        }
        public override bool Equals(object obj)
        {
            if (obj == null || obj is not NBitMask128) return false;
            return Equals((NBitMask128)obj);
        }
        public override int GetHashCode()
        {
            return mask0.GetHashCode() ^ mask1.GetHashCode();
        }
        public static bool operator ==(NBitMask128 a, NBitMask128 b)
        {
            return a.CompareTo(b) == 0;
        }
        public static bool operator !=(NBitMask128 a, NBitMask128 b)
        {
            return a.CompareTo(b) != 0;
        }

        public readonly static NBitMask128 Zero = new NBitMask128();
        public readonly static NBitMask128 One = new NBitMask128() { mask0 = -1, mask1 = -1 };
    }
    [BurstCompile]
    public struct NBitMask256 : IComparable<NBitMask256>, IEquatable<NBitMask256>
    {
        public long mask0;
        public long mask1;
        public long mask2;
        public long mask3;

        public bool isOnly1() => mask0 == -1 && mask1 == -1 && mask2 == -1 && mask3 == -1;
        public bool isOnly0() => mask0 == 0 && mask1 == 0 && mask2 == 0 && mask3 == 0;
        public void setBit1(int bitIndex)
        {
            if (bitIndex < 128)
            {
                if (bitIndex < 64)
                {
                    mask0 = NUtils.setBit1(mask0, bitIndex);
                }
                else
                {
                    mask1 = NUtils.setBit1(mask1, bitIndex % 64);
                }
            }
            else
            {
                if (bitIndex < 192)
                {
                    mask2 = NUtils.setBit1(mask2, bitIndex % 64);
                }
                else
                {
                    mask3 = NUtils.setBit1(mask3, bitIndex % 64);
                }
            }
        }
        public void setBit0(int bitIndex)
        {
            if (bitIndex < 128)
            {
                if (bitIndex < 64)
                {
                    mask0 = NUtils.setBit0(mask0, bitIndex);
                }
                else
                {
                    mask1 = NUtils.setBit0(mask1, bitIndex % 64);
                }
            }
            else
            {
                if (bitIndex < 192)
                {
                    mask2 = NUtils.setBit0(mask2, bitIndex % 64);
                }
                else
                {
                    mask3 = NUtils.setBit0(mask3, bitIndex % 64);
                }
            }
        }

        public int CompareTo(NBitMask256 other)
        {
            if (mask0 != other.mask0)
            {
                return mask0.CompareTo(other.mask0);
            }
            if (mask1 != other.mask1)
            {
                return mask1.CompareTo(other.mask1);
            }
            if (mask2 != other.mask2)
            {
                return mask2.CompareTo(other.mask2);
            }
            return mask3.CompareTo(other.mask3);
        }
        public bool Equals(NBitMask256 other)
        {
            return CompareTo(other) == 0;
        }
        public override bool Equals(object obj)
        {
            if (obj == null || obj is not NBitMask256) return false;
            return Equals((NBitMask256)obj);
        }
        public override int GetHashCode()
        {
            return mask0.GetHashCode() ^ mask1.GetHashCode() ^ mask2.GetHashCode() ^ mask3.GetHashCode();
        }
        public static bool operator ==(NBitMask256 a, NBitMask256 b)
        {
            return a.CompareTo(b) == 0;
        }
        public static bool operator !=(NBitMask256 a, NBitMask256 b)
        {
            return a.CompareTo(b) != 0;
        }
        public readonly static NBitMask256 Zero = new NBitMask256();
        public readonly static NBitMask256 One = new NBitMask256() { mask0 = -1, mask1 = -1, mask2 = -1, mask3 = -1 };
    }
    public struct NBitMask512 : IComparable<NBitMask512>, IEquatable<NBitMask512>
    {
        public NBitMask256 mask0;
        public NBitMask256 mask1;

        public bool isOnly1() => mask0.isOnly1() && mask1.isOnly1();
        public bool isOnly0() => mask0.isOnly0() && mask1.isOnly0();
        public void setBit1(int bitIndex)
        {
            if (bitIndex < 256)
            {
                mask0.setBit1(bitIndex);
            }
            else
            {
                mask1.setBit1(bitIndex % 256);
            }
        }
        public void setBit0(int bitIndex)
        {
            if (bitIndex < 256)
            {
                mask0.setBit0(bitIndex);
            }
            else
            {
                mask1.setBit0(bitIndex % 256);
            }
        }
        public int getBit1Index()
        {
            var bitIndex = NUtils.getBit1Index(mask0);
            if (bitIndex >= 0)
            {
                return bitIndex;
            }
            bitIndex = NUtils.getBit1Index(mask1);
            if (bitIndex >= 0)
            {
                return bitIndex + 256;
            }
            return bitIndex;
        }
        public int getBit0Index()
        {
            var bitIndex = NUtils.getBit0Index(mask0);
            if (bitIndex >= 0)
            {
                return bitIndex;
            }
            bitIndex = NUtils.getBit0Index(mask1);
            if (bitIndex >= 0)
            {
                return bitIndex + 256;
            }
            return bitIndex;
        }
        public bool checkBitMask(int bitIndex)
        {
            if (bitIndex < 256)
            {
                return NUtils.checkBitMask(mask0, bitIndex);
            }
            else
            {
                return NUtils.checkBitMask(mask1, bitIndex % 256);
            }
        }

        public int CompareTo(NBitMask512 other)
        {
            if (mask0 != other.mask0)
            {
                return mask0.CompareTo(other.mask0);
            }
            return mask1.CompareTo(other.mask1);
        }
        public bool Equals(NBitMask512 other)
        {
            return CompareTo(other) == 0;
        }
        public override bool Equals(object obj)
        {
            if (obj == null || obj is not NBitMask512) return false;
            return Equals((NBitMask512)obj);
        }
        public override int GetHashCode()
        {
            return mask0.GetHashCode() ^ mask1.GetHashCode();
        }
        public static bool operator ==(NBitMask512 a, NBitMask512 b)
        {
            return a.CompareTo(b) == 0;
        }
        public static bool operator !=(NBitMask512 a, NBitMask512 b)
        {
            return a.CompareTo(b) != 0;
        }
        public readonly static NBitMask512 Zero = new NBitMask512() { mask0 = NBitMask256.Zero, mask1 = NBitMask256.Zero };
        public readonly static NBitMask512 One = new NBitMask512() { mask0 = NBitMask256.One, mask1 = NBitMask256.One };
    }
}
