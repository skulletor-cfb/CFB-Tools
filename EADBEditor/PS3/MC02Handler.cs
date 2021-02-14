using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EA_DB_Editor.PS3
{
    public class EAChecksum
    {
        private enum Rotation
        {
            Left,
            Right
        }

        private bool Cached;

        private uint Value;

        private uint Last;

        private uint[] Table;

        private readonly uint[] DefaultTable;

        private static Exception BadBlockError = new Exception("Block size is invalid! The first block passed MUST be 4 bytes in length or greater. Once first block is cached this does not apply");

        private static Exception BadLengthError = new Exception("Supplied length is invalid! The first length value passed MUST be 4 or greater. Once first block is cached this does not apply");

        private static Exception BadTableError = new Exception("Provided table is invalid! Table size MUST be 1024 (0x400) in length!");

        public bool FirstBlockCached
        {
            get
            {
                return this.Cached;
            }
        }

        public uint[] CurrentTable
        {
            get
            {
                return this.Table;
            }
            set
            {
                if (value.Length != 1024)
                {
                    throw EAChecksum.BadTableError;
                }
                Array.Clear(this.Table, 0, 1024);
                this.Table = value;
            }
        }

        public uint GetHash
        {
            get
            {
                this.Value = ~this.Last;
                return this.Value;
            }
        }

        public byte[] GetHashBytes
        {
            get
            {
                byte[] bytes = BitConverter.GetBytes(this.GetHash);
                Array.Reverse(bytes);
                return bytes;
            }
        }

        public EAChecksum()
        {
            this.Cached = false;
            this.Value = 0u;
            this.Last = 0u;
            this.Table = new uint[1024];
            this.DefaultTable = new uint[]
            {
                0u,
                0u,
                0u,
                0u,
                4u,
                193u,
                29u,
                183u,
                9u,
                130u,
                59u,
                110u,
                13u,
                67u,
                38u,
                217u,
                19u,
                4u,
                118u,
                220u,
                23u,
                197u,
                107u,
                107u,
                26u,
                134u,
                77u,
                178u,
                30u,
                71u,
                80u,
                5u,
                38u,
                8u,
                237u,
                184u,
                34u,
                201u,
                240u,
                15u,
                47u,
                138u,
                214u,
                214u,
                43u,
                75u,
                203u,
                97u,
                53u,
                12u,
                155u,
                100u,
                49u,
                205u,
                134u,
                211u,
                60u,
                142u,
                160u,
                10u,
                56u,
                79u,
                189u,
                189u,
                76u,
                17u,
                219u,
                112u,
                72u,
                208u,
                198u,
                199u,
                69u,
                147u,
                224u,
                30u,
                65u,
                82u,
                253u,
                169u,
                95u,
                21u,
                173u,
                172u,
                91u,
                212u,
                176u,
                27u,
                86u,
                151u,
                150u,
                194u,
                82u,
                86u,
                139u,
                117u,
                106u,
                25u,
                54u,
                200u,
                110u,
                216u,
                43u,
                127u,
                99u,
                155u,
                13u,
                166u,
                103u,
                90u,
                16u,
                17u,
                121u,
                29u,
                64u,
                20u,
                125u,
                220u,
                93u,
                163u,
                112u,
                159u,
                123u,
                122u,
                116u,
                94u,
                102u,
                205u,
                152u,
                35u,
                182u,
                224u,
                156u,
                226u,
                171u,
                87u,
                145u,
                161u,
                141u,
                142u,
                149u,
                96u,
                144u,
                57u,
                139u,
                39u,
                192u,
                60u,
                143u,
                230u,
                221u,
                139u,
                130u,
                165u,
                251u,
                82u,
                134u,
                100u,
                230u,
                229u,
                190u,
                43u,
                91u,
                88u,
                186u,
                234u,
                70u,
                239u,
                183u,
                169u,
                96u,
                54u,
                179u,
                104u,
                125u,
                129u,
                173u,
                47u,
                45u,
                132u,
                169u,
                238u,
                48u,
                51u,
                164u,
                173u,
                22u,
                234u,
                160u,
                108u,
                11u,
                93u,
                212u,
                50u,
                109u,
                144u,
                208u,
                243u,
                112u,
                39u,
                221u,
                176u,
                86u,
                254u,
                217u,
                113u,
                75u,
                73u,
                199u,
                54u,
                27u,
                76u,
                195u,
                247u,
                6u,
                251u,
                206u,
                180u,
                32u,
                34u,
                202u,
                117u,
                61u,
                149u,
                242u,
                58u,
                128u,
                40u,
                246u,
                251u,
                157u,
                159u,
                251u,
                184u,
                187u,
                70u,
                255u,
                121u,
                166u,
                241u,
                225u,
                62u,
                246u,
                244u,
                229u,
                255u,
                235u,
                67u,
                232u,
                188u,
                205u,
                154u,
                236u,
                125u,
                208u,
                45u,
                52u,
                134u,
                112u,
                119u,
                48u,
                71u,
                109u,
                192u,
                61u,
                4u,
                75u,
                25u,
                57u,
                197u,
                86u,
                174u,
                39u,
                130u,
                6u,
                171u,
                35u,
                67u,
                27u,
                28u,
                46u,
                0u,
                61u,
                197u,
                42u,
                193u,
                32u,
                114u,
                18u,
                142u,
                157u,
                207u,
                22u,
                79u,
                128u,
                120u,
                27u,
                12u,
                166u,
                161u,
                31u,
                205u,
                187u,
                22u,
                1u,
                138u,
                235u,
                19u,
                5u,
                75u,
                246u,
                164u,
                8u,
                8u,
                208u,
                125u,
                12u,
                201u,
                205u,
                202u,
                120u,
                151u,
                171u,
                7u,
                124u,
                86u,
                182u,
                176u,
                113u,
                21u,
                144u,
                105u,
                117u,
                212u,
                141u,
                222u,
                107u,
                147u,
                221u,
                219u,
                111u,
                82u,
                192u,
                108u,
                98u,
                17u,
                230u,
                181u,
                102u,
                208u,
                251u,
                2u,
                94u,
                159u,
                70u,
                191u,
                90u,
                94u,
                91u,
                8u,
                87u,
                29u,
                125u,
                209u,
                83u,
                220u,
                96u,
                102u,
                77u,
                155u,
                48u,
                99u,
                73u,
                90u,
                45u,
                212u,
                68u,
                25u,
                11u,
                13u,
                64u,
                216u,
                22u,
                186u,
                172u,
                165u,
                198u,
                151u,
                168u,
                100u,
                219u,
                32u,
                165u,
                39u,
                253u,
                249u,
                161u,
                230u,
                224u,
                78u,
                191u,
                161u,
                176u,
                75u,
                187u,
                96u,
                173u,
                252u,
                182u,
                35u,
                139u,
                37u,
                178u,
                226u,
                150u,
                146u,
                138u,
                173u,
                43u,
                47u,
                142u,
                108u,
                54u,
                152u,
                131u,
                47u,
                16u,
                65u,
                135u,
                238u,
                13u,
                246u,
                153u,
                169u,
                93u,
                243u,
                157u,
                104u,
                64u,
                68u,
                144u,
                43u,
                102u,
                157u,
                148u,
                234u,
                123u,
                42u,
                224u,
                180u,
                29u,
                231u,
                228u,
                117u,
                0u,
                80u,
                233u,
                54u,
                38u,
                137u,
                237u,
                247u,
                59u,
                62u,
                243u,
                176u,
                107u,
                59u,
                247u,
                113u,
                118u,
                140u,
                250u,
                50u,
                80u,
                85u,
                254u,
                243u,
                77u,
                226u,
                198u,
                188u,
                240u,
                95u,
                194u,
                125u,
                237u,
                232u,
                207u,
                62u,
                203u,
                49u,
                203u,
                255u,
                214u,
                134u,
                213u,
                184u,
                134u,
                131u,
                209u,
                121u,
                155u,
                52u,
                220u,
                58u,
                189u,
                237u,
                216u,
                251u,
                160u,
                90u,
                105u,
                12u,
                224u,
                238u,
                109u,
                205u,
                253u,
                89u,
                96u,
                142u,
                219u,
                128u,
                100u,
                79u,
                198u,
                55u,
                122u,
                8u,
                150u,
                50u,
                126u,
                201u,
                139u,
                133u,
                115u,
                138u,
                173u,
                92u,
                119u,
                75u,
                176u,
                235u,
                79u,
                4u,
                13u,
                86u,
                75u,
                197u,
                16u,
                225u,
                70u,
                134u,
                54u,
                56u,
                66u,
                71u,
                43u,
                143u,
                92u,
                0u,
                123u,
                138u,
                88u,
                193u,
                102u,
                61u,
                85u,
                130u,
                64u,
                228u,
                81u,
                67u,
                93u,
                83u,
                37u,
                29u,
                59u,
                158u,
                33u,
                220u,
                38u,
                41u,
                44u,
                159u,
                0u,
                240u,
                40u,
                94u,
                29u,
                71u,
                54u,
                25u,
                77u,
                66u,
                50u,
                216u,
                80u,
                245u,
                63u,
                155u,
                118u,
                44u,
                59u,
                90u,
                107u,
                155u,
                3u,
                21u,
                214u,
                38u,
                7u,
                212u,
                203u,
                145u,
                10u,
                151u,
                237u,
                72u,
                14u,
                86u,
                240u,
                255u,
                16u,
                17u,
                160u,
                250u,
                20u,
                208u,
                189u,
                77u,
                25u,
                147u,
                155u,
                148u,
                29u,
                82u,
                134u,
                35u,
                241u,
                47u,
                86u,
                14u,
                245u,
                238u,
                75u,
                185u,
                248u,
                173u,
                109u,
                96u,
                252u,
                108u,
                112u,
                215u,
                226u,
                43u,
                32u,
                210u,
                230u,
                234u,
                61u,
                101u,
                235u,
                169u,
                27u,
                188u,
                239u,
                104u,
                6u,
                11u,
                215u,
                39u,
                187u,
                182u,
                211u,
                230u,
                166u,
                1u,
                222u,
                165u,
                128u,
                216u,
                218u,
                100u,
                157u,
                111u,
                196u,
                35u,
                205u,
                106u,
                192u,
                226u,
                208u,
                221u,
                205u,
                161u,
                246u,
                4u,
                201u,
                96u,
                235u,
                179u,
                189u,
                62u,
                141u,
                126u,
                185u,
                255u,
                144u,
                201u,
                180u,
                188u,
                182u,
                16u,
                176u,
                125u,
                171u,
                167u,
                174u,
                58u,
                251u,
                162u,
                170u,
                251u,
                230u,
                21u,
                167u,
                184u,
                192u,
                204u,
                163u,
                121u,
                221u,
                123u,
                155u,
                54u,
                96u,
                198u,
                159u,
                247u,
                125u,
                113u,
                146u,
                180u,
                91u,
                168u,
                150u,
                117u,
                70u,
                31u,
                136u,
                50u,
                22u,
                26u,
                140u,
                243u,
                11u,
                173u,
                129u,
                176u,
                45u,
                116u,
                133u,
                113u,
                48u,
                195u,
                93u,
                138u,
                144u,
                153u,
                89u,
                75u,
                141u,
                46u,
                84u,
                8u,
                171u,
                247u,
                80u,
                201u,
                182u,
                64u,
                78u,
                142u,
                230u,
                69u,
                74u,
                79u,
                251u,
                242u,
                71u,
                12u,
                221u,
                43u,
                67u,
                205u,
                192u,
                156u,
                123u,
                130u,
                125u,
                33u,
                127u,
                67u,
                96u,
                150u,
                114u,
                0u,
                70u,
                79u,
                118u,
                193u,
                91u,
                248u,
                104u,
                134u,
                11u,
                253u,
                108u,
                71u,
                22u,
                74u,
                97u,
                4u,
                48u,
                147u,
                101u,
                197u,
                45u,
                36u,
                17u,
                155u,
                75u,
                233u,
                21u,
                90u,
                86u,
                94u,
                24u,
                25u,
                112u,
                135u,
                28u,
                216u,
                109u,
                48u,
                2u,
                159u,
                61u,
                53u,
                6u,
                94u,
                32u,
                130u,
                11u,
                29u,
                6u,
                91u,
                15u,
                220u,
                27u,
                236u,
                55u,
                147u,
                166u,
                81u,
                51u,
                82u,
                187u,
                230u,
                62u,
                17u,
                157u,
                63u,
                58u,
                208u,
                128u,
                136u,
                36u,
                151u,
                208u,
                141u,
                32u,
                86u,
                205u,
                58u,
                45u,
                21u,
                235u,
                227u,
                41u,
                212u,
                246u,
                84u,
                197u,
                169u,
                38u,
                121u,
                193u,
                104u,
                59u,
                206u,
                204u,
                43u,
                29u,
                23u,
                200u,
                234u,
                0u,
                160u,
                214u,
                173u,
                80u,
                165u,
                210u,
                108u,
                77u,
                18u,
                223u,
                47u,
                107u,
                203u,
                219u,
                238u,
                118u,
                124u,
                227u,
                161u,
                203u,
                193u,
                231u,
                96u,
                214u,
                118u,
                234u,
                35u,
                240u,
                175u,
                238u,
                226u,
                237u,
                24u,
                240u,
                165u,
                189u,
                29u,
                244u,
                100u,
                160u,
                170u,
                249u,
                39u,
                134u,
                115u,
                253u,
                230u,
                155u,
                196u,
                137u,
                184u,
                253u,
                9u,
                141u,
                121u,
                224u,
                190u,
                128u,
                58u,
                198u,
                103u,
                132u,
                251u,
                219u,
                208u,
                154u,
                188u,
                139u,
                213u,
                158u,
                125u,
                150u,
                98u,
                147u,
                62u,
                176u,
                187u,
                151u,
                255u,
                173u,
                12u,
                175u,
                176u,
                16u,
                177u,
                171u,
                113u,
                13u,
                6u,
                166u,
                50u,
                43u,
                223u,
                162u,
                243u,
                54u,
                104u,
                188u,
                180u,
                102u,
                109u,
                184u,
                117u,
                123u,
                218u,
                181u,
                54u,
                93u,
                3u,
                177u,
                247u,
                64u,
                180u
            };
            this.Reset();
        }

        public EAChecksum(uint[] CustomTable) : this()
        {
            this.CurrentTable = CustomTable;
        }

        private void CacheFirstBlock(byte[] Data)
        {
            if (this.Cached)
            {
                return;
            }
            uint num = this.Rotate((uint)Data[0], 8u, EAChecksum.Rotation.Right) | this.Rotate((uint)Data[1], 16u, EAChecksum.Rotation.Right);
            uint num2 = num | this.Rotate((uint)Data[2], 8u, EAChecksum.Rotation.Left);
            this.Last = ~(num2 | (uint)Data[3]);
            this.Cached = true;
        }

        public void TransformBlock(byte[] Data)
        {
            this.TransformBlock(Data, checked((ulong)Data.Length));
        }

        public void TransformBlock(byte[] Data, ulong Length)
        {
            if (!this.Cached)
            {
                if (Data.Length < 4)
                {
                    throw EAChecksum.BadBlockError;
                }
                if (decimal.Compare(new decimal(Length), new decimal(4L)) < 0)
                {
                    throw EAChecksum.BadLengthError;
                }
            }
            this.Compute(Data, Length);
        }

        public uint TransformFinalBlock()
        {
            return this.GetHash;
        }

        public uint TransformFinalBlock(byte[] Data)
        {
            return this.TransformFinalBlock(Data, checked((ulong)Data.Length));
        }

        public uint TransformFinalBlock(byte[] Data, ulong Length)
        {
            if (!this.Cached)
            {
                if (Data.Length < 4)
                {
                    throw EAChecksum.BadBlockError;
                }
                if (decimal.Compare(new decimal(Length), new decimal(4L)) < 0)
                {
                    throw EAChecksum.BadLengthError;
                }
            }
            this.Compute(Data, Length);
            return this.GetHash;
        }

        private void Compute(byte[] Data, ulong Length)
        {
            checked
            {
                if (Data != null)
                {
                    if (Data.Length == 0)
                    {
                        return;
                    }
                    if (decimal.Compare(new decimal(Length), decimal.Zero) == 0)
                    {
                        return;
                    }
                    ulong num = 0uL;
                    if (!this.Cached)
                    {
                        this.CacheFirstBlock(Data);
                        num = 4uL;
                    }
                    ulong arg_4D_0 = num;
                    ulong num2 = Convert.ToUInt64(decimal.Subtract(new decimal(Length), decimal.One));
                    for (num = arg_4D_0; num <= num2; num += 1uL)
                    {
                        uint num3 = (uint)(unchecked((ulong)this.Rotate(this.Last, 10u, EAChecksum.Rotation.Left)) & 1020uL);
                        this.Last <<= 8;
                        this.Last |= (uint)Data[(int)num];
                        num3 = this.NextIndex(num3);
                        this.Last ^= num3;
                    }
                }
            }
        }

        private uint Rotate(uint Value, uint Shift, EAChecksum.Rotation Direction)
        {
            checked
            {
                Shift = (uint)(unchecked((ulong)Shift) & 31uL);
                if (Direction == EAChecksum.Rotation.Left)
                {
                    Value = (Value >> (int)(32uL - unchecked((ulong)Shift)) | Value << (int)Shift);
                }
                else
                {
                    Value = (Value << (int)(32uL - unchecked((ulong)Shift)) | Value >> (int)Shift);
                }
                return Value;
            }
        }

        private uint NextIndex(uint Index)
        {
            byte[] array = new byte[4];
            byte b = 0;
            do
            {
                array[(int)b] = checked((byte)this.CurrentTable[(int)(Index + (uint)b)]);
                b += 1;
            }
            while (b <= 3);
            Array.Reverse(array);
            return BitConverter.ToUInt32(array, 0);
        }

        public void Reset()
        {
            this.Reset(this.DefaultTable);
        }

        public void Reset(uint[] CustomTable)
        {
            this.Cached = false;
            this.Value = 0u;
            this.Last = 0u;
            this.CurrentTable = CustomTable;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }

    public class PackageHeader
    {
        private readonly byte[] Magic;

        private byte[] Header;

        private readonly Exception BadHeaderData;

        private readonly Exception BadHeaderSize;

        private readonly Exception BadHeaderMagic;

        public byte[] GetBytes
        {
            get
            {
                if (this.Header == null)
                {
                    throw this.BadHeaderData;
                }
                byte[] array = new byte[(int)(checked(this.HeaderSize - 1 + 1))];
                Array.Copy(this.Header, array, (int)this.HeaderSize);
                return array;
            }
        }

        public uint SaveDescriptorSize
        {
            get
            {
                if (this.Header == null)
                {
                    throw this.BadHeaderData;
                }
                byte[] array = new byte[4];
                Array.Copy(this.Header, 8, array, 0, 4);
                return this.ReverseBytesToUInt32(array);
            }
            set
            {
                if (this.Header == null)
                {
                    throw this.BadHeaderData;
                }
                Array.Copy(this.UInt32ToBytesReverse(value), 0, this.Header, 8, 4);
            }
        }

        public int SaveDescriptorOffset
        {
            get
            {
                return 28;
            }
        }

        public uint SaveDataSize
        {
            get
            {
                if (this.Header == null)
                {
                    throw this.BadHeaderData;
                }
                byte[] array = new byte[4];
                Array.Copy(this.Header, 12, array, 0, 4);
                return this.ReverseBytesToUInt32(array);
            }
            set
            {
                if (this.Header == null)
                {
                    throw this.BadHeaderData;
                }
                Array.Copy(this.UInt32ToBytesReverse(value), 0, this.Header, 12, 4);
            }
        }

        public int SaveDataOffset
        {
            get
            {
                return checked((int)(28uL + unchecked((ulong)this.SaveDescriptorSize)));
            }
        }

        public uint PackageSize
        {
            get
            {
                if (this.Header == null)
                {
                    throw this.BadHeaderData;
                }
                byte[] array = new byte[4];
                Array.Copy(this.Header, 4, array, 0, 4);
                return this.ReverseBytesToUInt32(array);
            }
            set
            {
                if (this.Header == null)
                {
                    throw this.BadHeaderData;
                }
                Array.Copy(this.UInt32ToBytesReverse(value), 0, this.Header, 4, 4);
            }
        }

        public byte HeaderSize
        {
            get
            {
                return 28;
            }
        }

        public byte HeaderOffset
        {
            get
            {
                return 0;
            }
        }

        public byte[] SaveDescriptorHash
        {
            get
            {
                if (this.Header == null)
                {
                    throw this.BadHeaderData;
                }
                byte[] array = new byte[4];
                Array.Copy(this.Header, 16, array, 0, 4);
                return array;
            }
            set
            {
                if (this.Header == null)
                {
                    throw this.BadHeaderData;
                }
                Array.ConstrainedCopy(value, 0, this.Header, 16, 4);
            }
        }

        public byte[] SaveDataHash
        {
            get
            {
                if (this.Header == null)
                {
                    throw this.BadHeaderData;
                }
                byte[] array = new byte[4];
                Array.Copy(this.Header, 20, array, 0, 4);
                return array;
            }
            set
            {
                if (this.Header == null)
                {
                    throw this.BadHeaderData;
                }
                Array.ConstrainedCopy(value, 0, this.Header, 20, 4);
            }
        }

        public byte[] HeaderHash
        {
            get
            {
                if (this.Header == null)
                {
                    throw this.BadHeaderData;
                }
                byte[] array = new byte[4];
                Array.Copy(this.Header, 24, array, 0, 4);
                return array;
            }
            set
            {
                if (this.Header == null)
                {
                    throw this.BadHeaderData;
                }
                Array.ConstrainedCopy(value, 0, this.Header, 24, 4);
            }
        }

        public PackageHeader(byte[] MC02Header)
        {
            this.Magic = new byte[]
            {
                77,
                67,
                48,
                50
            };
            this.Header = null;
            this.BadHeaderData = new Exception("Header data is null");
            this.BadHeaderSize = new Exception("Header data is incorrent length");
            this.BadHeaderMagic = new Exception("Header does not contain the MC02 magic. Invalid package");
            if (MC02Header.Length != (int)this.HeaderSize)
            {
                throw this.BadHeaderSize;
            }
            this.Header = new byte[(int)(checked(this.HeaderSize - 1 + 1))];
            Array.Copy(MC02Header, 0, this.Header, 0, (int)this.HeaderSize);
            this.Validate();
        }

        public PackageHeader(uint SaveDescriptorSize, uint SaveDataSize)
        {
            this.Magic = new byte[]
            {
                77,
                67,
                48,
                50
            };
            this.Header = null;
            this.BadHeaderData = new Exception("Header data is null");
            this.BadHeaderSize = new Exception("Header data is incorrent length");
            this.BadHeaderMagic = new Exception("Header does not contain the MC02 magic. Invalid package");
            checked
            {
                this.Header = new byte[(int)(this.HeaderSize - 1 + 1)];
                Array.Copy(this.Magic, 0, this.Header, 0, 4);
                this.PackageSize = SaveDescriptorSize + SaveDataSize + (uint)this.HeaderSize;
                this.SaveDescriptorSize = SaveDescriptorSize;
                this.SaveDataSize = SaveDataSize;
            }
        }

        public void Dispose()
        {
            if (this.Header != null)
            {
                Array.Clear(this.Header, 0, this.Header.Length);
            }
            GC.SuppressFinalize(this);
        }

        private byte[] UInt32ToBytesReverse(uint value)
        {
            byte[] array = new byte[4];
            Array.Copy(BitConverter.GetBytes(value), array, 4);
            Array.Reverse(array);
            return array;
        }

        private uint ReverseBytesToUInt32(byte[] value)
        {
            Array.Reverse(value);
            return BitConverter.ToUInt32(value, 0);
        }

        private void Validate()
        {
            if (this.Header == null)
            {
                throw this.BadHeaderData;
            }

            return; 

            // skip magic check
            byte[] array = new byte[4];
            Array.Copy(this.Header, 0, array, 0, 4);
            byte b = 0;
            while (array[(int)b] == this.Magic[(int)b])
            {
                b += 1;
                if (b > 3)
                {
                    Array.Clear(array, 0, 4);
                    return;
                }
            }
            throw this.BadHeaderMagic;
        }
    }

    public class Package
    {
        public enum DataType
        {
            SaveDescriptor,
            SaveData
        }

        private byte[] xPackage;

        private byte[] xDescriptor;

        private byte[] xSave;

        private PackageHeader Header;

        private EAChecksum CRC;

        private readonly Exception BadInputData;

        private readonly Exception BadInputFile;

        private readonly Exception BadDescriptorFile;

        private readonly Exception BadSaveDataFile;

        private readonly Exception BadDescriptorSize;

        private readonly Exception BadSaveDataSize;

        private readonly Exception BadPackage;

        private readonly Exception BadData;

        public byte HeaderSize
        {
            get
            {
                return this.Header.HeaderSize;
            }
        }

        public byte HeaderOffset
        {
            get
            {
                return this.Header.HeaderOffset;
            }
        }

        public byte[] HeaderHash
        {
            get
            {
                return this.Header.HeaderHash;
            }
        }

        public byte[] HeaderBytes
        {
            get
            {
                return this.Header.GetBytes;
            }
        }

        public uint SaveDataSize
        {
            get
            {
                return this.Header.SaveDataSize;
            }
        }

        public int SaveDataOffset
        {
            get
            {
                return this.Header.SaveDataOffset;
            }
        }

        public byte[] SaveDataHash
        {
            get
            {
                return this.Header.SaveDataHash;
            }
        }

        public uint SaveDescriptorSize
        {
            get
            {
                return this.Header.SaveDescriptorSize;
            }
        }

        public int SaveDescriptorOffset
        {
            get
            {
                return this.Header.SaveDescriptorOffset;
            }
        }

        public byte[] SaveDescriptorHash
        {
            get
            {
                return this.Header.SaveDescriptorHash;
            }
        }

        public uint PackageSize
        {
            get
            {
                return this.Header.PackageSize;
            }
        }

        public Package(byte[] MC02Package)
        {
            this.xPackage = null;
            this.xDescriptor = null;
            this.xSave = null;
            this.Header = null;
            this.CRC = null;
            this.BadInputData = new Exception("Input data is null");
            this.BadInputFile = new Exception("Input file was not found or could not be accessed!");
            this.BadDescriptorFile = new Exception("Input save descriptor file was not found or could not be accessed!");
            this.BadSaveDataFile = new Exception("Input save data file was not found or could not be accessed!");
            this.BadDescriptorSize = new Exception("Save descriptor is zero bytes in length!");
            this.BadSaveDataSize = new Exception("Save data is zero bytes in length!");
            this.BadPackage = new Exception("MC02 package is corrupt!");
            this.BadData = new Exception("Working data is null");
            if (MC02Package == null)
            {
                throw this.BadInputData;
            }
            if (MC02Package.Length <= 28)
            {
                throw this.BadPackage;
            }
            this.xPackage = new byte[checked(MC02Package.Length - 1 + 1)];
            Array.Copy(MC02Package, this.xPackage, MC02Package.Length);
            this.Parse();
        }

        public Package(string MC02Package)
        {
            this.xPackage = null;
            this.xDescriptor = null;
            this.xSave = null;
            this.Header = null;
            this.CRC = null;
            this.BadInputData = new Exception("Input data is null");
            this.BadInputFile = new Exception("Input file was not found or could not be accessed!");
            this.BadDescriptorFile = new Exception("Input save descriptor file was not found or could not be accessed!");
            this.BadSaveDataFile = new Exception("Input save data file was not found or could not be accessed!");
            this.BadDescriptorSize = new Exception("Save descriptor is zero bytes in length!");
            this.BadSaveDataSize = new Exception("Save data is zero bytes in length!");
            this.BadPackage = new Exception("MC02 package is corrupt!");
            this.BadData = new Exception("Working data is null");
            if (!File.Exists(MC02Package))
            {
                throw this.BadInputFile;
            }
            if (new FileInfo(MC02Package).Length <= 28L)
            {
                throw this.BadPackage;
            }
            FileStream fileStream = new FileStream(MC02Package, FileMode.Open, FileAccess.Read, FileShare.None);
            checked
            {
                this.xPackage = new byte[(int)(fileStream.Length - 1L) + 1];
                fileStream.Read(this.xPackage, 0, (int)fileStream.Length);
                fileStream.Close();
                this.Parse();
            }
        }

        public Package(byte[] SaveDescriptor, byte[] SaveData)
        {
            this.xPackage = null;
            this.xDescriptor = null;
            this.xSave = null;
            this.Header = null;
            this.CRC = null;
            this.BadInputData = new Exception("Input data is null");
            this.BadInputFile = new Exception("Input file was not found or could not be accessed!");
            this.BadDescriptorFile = new Exception("Input save descriptor file was not found or could not be accessed!");
            this.BadSaveDataFile = new Exception("Input save data file was not found or could not be accessed!");
            this.BadDescriptorSize = new Exception("Save descriptor is zero bytes in length!");
            this.BadSaveDataSize = new Exception("Save data is zero bytes in length!");
            this.BadPackage = new Exception("MC02 package is corrupt!");
            this.BadData = new Exception("Working data is null");
            if (SaveDescriptor.Length < 0)
            {
                throw this.BadDescriptorSize;
            }
            if (SaveData.Length < 0)
            {
                throw this.BadSaveDataSize;
            }
            checked
            {
                this.xDescriptor = new byte[SaveDescriptor.Length - 1 + 1];
                this.xSave = new byte[SaveData.Length - 1 + 1];
                Array.Copy(SaveDescriptor, this.xDescriptor, SaveDescriptor.Length);
                Array.Copy(SaveData, this.xSave, SaveData.Length);
                this.Create(false, null);
            }
        }

        public Package(string SaveDescriptor, string SaveData)
        {
            this.xPackage = null;
            this.xDescriptor = null;
            this.xSave = null;
            this.Header = null;
            this.CRC = null;
            this.BadInputData = new Exception("Input data is null");
            this.BadInputFile = new Exception("Input file was not found or could not be accessed!");
            this.BadDescriptorFile = new Exception("Input save descriptor file was not found or could not be accessed!");
            this.BadSaveDataFile = new Exception("Input save data file was not found or could not be accessed!");
            this.BadDescriptorSize = new Exception("Save descriptor is zero bytes in length!");
            this.BadSaveDataSize = new Exception("Save data is zero bytes in length!");
            this.BadPackage = new Exception("MC02 package is corrupt!");
            this.BadData = new Exception("Working data is null");
            if (!File.Exists(SaveDescriptor))
            {
                throw this.BadDescriptorFile;
            }
            if (!File.Exists(SaveData))
            {
                throw this.BadSaveDataFile;
            }
            if (new FileInfo(SaveDescriptor).Length < 0L)
            {
                throw this.BadDescriptorSize;
            }
            if (new FileInfo(SaveData).Length < 0L)
            {
                throw this.BadSaveDataSize;
            }
            checked
            {
                this.xDescriptor = new byte[(int)(new FileInfo(SaveDescriptor).Length - 1L) + 1];
                this.xSave = new byte[(int)(new FileInfo(SaveData).Length - 1L) + 1];
                Array.Copy(File.ReadAllBytes(SaveDescriptor), this.xDescriptor, this.xDescriptor.Length);
                Array.Copy(File.ReadAllBytes(SaveData), this.xSave, this.xSave.Length);
                this.Create(false, null);
            }
        }

        private void Create(bool Hashing = false, uint[] CustomTable = null)
        {
            checked
            {
                this.Header = new PackageHeader((uint)this.xDescriptor.Length, (uint)this.xSave.Length);
                this.xPackage = new byte[(int)(unchecked((ulong)this.Header.PackageSize) - 1uL) + 1];
                if (Hashing)
                {
                    this.HashPackage(CustomTable);
                }
                Array.Copy(this.Header.GetBytes, this.xPackage, 28);
            }
            Array.Copy(this.xDescriptor, 0L, this.xPackage, 28L, (long)((ulong)this.Header.SaveDescriptorSize));
            Array.Copy(this.xSave, 0L, this.xPackage, (long)(checked((int)(28uL + unchecked((ulong)this.Header.SaveDescriptorSize)))), (long)((ulong)this.Header.SaveDataSize));
        }

        private void Parse()
        {
            byte[] array = new byte[28];
            Array.Copy(this.xPackage, 0, array, 0, 28);
            this.Header = new PackageHeader(array);
            Array.Clear(array, 0, 28);
            this.xDescriptor = new byte[checked((int)(unchecked((ulong)this.Header.SaveDescriptorSize) - 1uL) + 1)];
            Array.Copy(this.xPackage, 28L, this.xDescriptor, 0L, (long)((ulong)this.Header.SaveDescriptorSize));
            this.xSave = new byte[checked((int)(unchecked((ulong)this.Header.SaveDataSize) - 1uL) + 1)];
            Array.Copy(this.xPackage, (long)(checked((int)(28uL + unchecked((ulong)this.Header.SaveDescriptorSize)))), this.xSave, 0L, (long)((ulong)this.Header.SaveDataSize));
        }

        private void HashPackage(uint[] CustomTable = null)
        {
            this.HashDescriptor(CustomTable);
            this.HashSaveData(CustomTable);
            this.HashHeader(CustomTable);
        }

        private void HashHeader(uint[] CustomTable = null)
        {
            if (this.Header == null)
            {
                return;
            }
            if (CustomTable == null)
            {
                this.CRC = new EAChecksum();
            }
            else
            {
                this.CRC = new EAChecksum(CustomTable);
            }
            byte[] array = new byte[24];
            byte[] array2 = new byte[4];
            Array.Copy(this.Header.GetBytes, array, array.Length);
            this.CRC.TransformBlock(array);
            Array.Clear(array, 0, array.Length);
            Array.Copy(this.CRC.GetHashBytes, array2, 4);
            this.CRC.Dispose();
            this.CRC = null;
            this.Header.HeaderHash = array2;
        }

        private void HashDescriptor(uint[] CustomTable = null)
        {
            if (this.xDescriptor == null)
            {
                return;
            }
            if (CustomTable == null)
            {
                this.CRC = new EAChecksum();
            }
            else
            {
                this.CRC = new EAChecksum(CustomTable);
            }
            byte[] array = new byte[checked(this.xDescriptor.Length - 1 + 1)];
            byte[] array2 = new byte[4];
            Array.Copy(this.xDescriptor, array, array.Length);
            this.CRC.TransformBlock(array);
            Array.Clear(array, 0, array.Length);
            Array.Copy(this.CRC.GetHashBytes, array2, 4);
            this.CRC.Dispose();
            this.CRC = null;
            this.Header.SaveDescriptorHash = array2;
        }

        private void HashSaveData(uint[] CustomTable = null)
        {
            if (this.xSave == null)
            {
                return;
            }
            if (CustomTable == null)
            {
                this.CRC = new EAChecksum();
            }
            else
            {
                this.CRC = new EAChecksum(CustomTable);
            }
            byte[] array = new byte[checked(this.xSave.Length - 1 + 1)];
            byte[] array2 = new byte[4];
            Array.Copy(this.xSave, array, array.Length);
            this.CRC.TransformBlock(array);
            Array.Clear(array, 0, array.Length);
            Array.Copy(this.CRC.GetHashBytes, array2, 4);
            this.CRC.Dispose();
            this.CRC = null;
            this.Header.SaveDataHash = array2;
        }

        private bool CompareChunks(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }
            ulong arg_13_0 = 0uL;
            checked
            {
                ulong num = (ulong)(a.Length - 1);
                for (ulong num2 = arg_13_0; num2 <= num; num2 += 1uL)
                {
                    if (a[(int)num2] != b[(int)num2])
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public bool ValidHashes(uint[] CustomTable = null)
        {
            return this.ValidateHeader(CustomTable) | this.ValidateDescriptor(CustomTable) | this.ValidateSaveData(CustomTable);
        }

        public bool ValidateHeader(uint[] CustomTable = null)
        {
            if (this.Header == null)
            {
                throw this.BadInputData;
            }
            if (CustomTable == null)
            {
                this.CRC = new EAChecksum();
            }
            else
            {
                this.CRC = new EAChecksum(CustomTable);
            }
            byte[] array = new byte[4];
            byte[] array2 = new byte[24];
            Array.Copy(this.Header.HeaderHash, array, 4);
            Array.Copy(this.Header.GetBytes, array2, array2.Length);
            this.CRC.TransformBlock(array2);
            Array.Clear(array2, 0, array2.Length);
            bool result = this.CompareChunks(this.CRC.GetHashBytes, array);
            this.CRC.Dispose();
            this.CRC = null;
            return result;
        }

        public bool ValidateDescriptor(uint[] CustomTable = null)
        {
            if (this.xDescriptor == null)
            {
                throw this.BadInputData;
            }
            if (CustomTable == null)
            {
                this.CRC = new EAChecksum();
            }
            else
            {
                this.CRC = new EAChecksum(CustomTable);
            }
            byte[] array = new byte[4];
            byte[] array2 = new byte[checked(this.xDescriptor.Length - 1 + 1)];
            Array.Copy(this.Header.SaveDescriptorHash, array, 4);
            Array.Copy(this.xDescriptor, array2, array2.Length);
            this.CRC.TransformBlock(array2);
            Array.Clear(array2, 0, array2.Length);
            bool result = this.CompareChunks(this.CRC.GetHashBytes, array);
            this.CRC.Dispose();
            this.CRC = null;
            return result;
        }

        public bool ValidateSaveData(uint[] CustomTable = null)
        {
            if (this.xSave == null)
            {
                throw this.BadInputData;
            }
            if (CustomTable == null)
            {
                this.CRC = new EAChecksum();
            }
            else
            {
                this.CRC = new EAChecksum(CustomTable);
            }
            byte[] array = new byte[4];
            byte[] array2 = new byte[checked(this.xSave.Length - 1 + 1)];
            Array.Copy(this.Header.SaveDataHash, array, 4);
            Array.Copy(this.xSave, array2, array2.Length);
            this.CRC.TransformBlock(array2);
            Array.Clear(array2, 0, array2.Length);
            bool result = this.CompareChunks(this.CRC.GetHashBytes, array);
            this.CRC.Dispose();
            this.CRC = null;
            return result;
        }

        public byte[] Extract(Package.DataType Type)
        {
            checked
            {
                byte[] array;
                if (Type == Package.DataType.SaveData)
                {
                    array = new byte[this.xSave.Length - 1 + 1];
                    Array.Copy(this.xSave, array, array.Length);
                }
                else
                {
                    array = new byte[this.xDescriptor.Length - 1 + 1];
                    Array.Copy(this.xDescriptor, array, array.Length);
                }
                return array;
            }
        }

        public void Extract(Package.DataType Type, string OutputFile)
        {
            FileStream fileStream = new FileStream(OutputFile, FileMode.Create, FileAccess.Write, FileShare.None);
            checked
            {
                byte[] array;
                if (Type == Package.DataType.SaveData)
                {
                    array = new byte[this.xSave.Length - 1 + 1];
                    Array.Copy(this.xSave, array, array.Length);
                }
                else
                {
                    array = new byte[this.xDescriptor.Length - 1 + 1];
                    Array.Copy(this.xDescriptor, array, array.Length);
                }
                fileStream.Write(array, 0, array.Length);
                fileStream.Flush();
                fileStream.Close();
                Array.Clear(array, 0, array.Length);
            }
        }

        public void Overwrite(Package.DataType Type, byte[] InputData)
        {
            checked
            {
                if (Type == Package.DataType.SaveData)
                {
                    this.xSave = new byte[InputData.Length - 1 + 1];
                    Array.Copy(InputData, this.xSave, InputData.Length);
                }
                else
                {
                    this.xDescriptor = new byte[InputData.Length - 1 + 1];
                    Array.Copy(InputData, this.xDescriptor, InputData.Length);
                }
            }
        }

        public void Overwrite(Package.DataType Type, string InputFile)
        {
            FileStream fileStream = new FileStream(InputFile, FileMode.Open, FileAccess.Read, FileShare.None);
            checked
            {
                byte[] array = new byte[(int)(fileStream.Length - 1L) + 1];
                fileStream.Read(array, 0, array.Length);
                fileStream.Close();
                if (Type == Package.DataType.SaveData)
                {
                    this.xSave = new byte[array.Length - 1 + 1];
                    Array.Copy(array, this.xSave, array.Length);
                }
                else
                {
                    this.xDescriptor = new byte[array.Length - 1 + 1];
                    Array.Copy(array, this.xDescriptor, array.Length);
                }
            }
        }

        public byte[] Save(bool HashPackage = false, uint[] CustomTable = null)
        {
            this.Create(HashPackage, CustomTable);
            byte[] array = new byte[checked(this.xPackage.Length - 1 + 1)];
            Array.Copy(this.xPackage, array, array.Length);
            return array;
        }

        public void Save(string OutputFile, bool HashPackage = false, uint[] CustomTable = null)
        {
            this.Create(HashPackage, CustomTable);
            FileStream fileStream = new FileStream(OutputFile, FileMode.Create, FileAccess.Write, FileShare.None);
            fileStream.Write(this.xPackage, 0, this.xPackage.Length);
            fileStream.Flush();
            fileStream.Close();
        }

        public void Dispose()
        {
            if (this.xPackage != null)
            {
                Array.Clear(this.xPackage, 0, this.xPackage.Length);
            }
            if (this.xSave != null)
            {
                Array.Clear(this.xSave, 0, this.xSave.Length);
            }
            if (this.xDescriptor != null)
            {
                Array.Clear(this.xDescriptor, 0, this.xDescriptor.Length);
            }
            if (this.Header != null)
            {
                this.Header.Dispose();
            }
            if (this.CRC != null)
            {
                this.CRC.Dispose();
            }
            GC.SuppressFinalize(this);
        }
    }
}
