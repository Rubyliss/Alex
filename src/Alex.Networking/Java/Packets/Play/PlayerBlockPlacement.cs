﻿using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Utils;
using Alex.Networking.Java.Util;
using Microsoft.Xna.Framework;
using MiNET;

namespace Alex.Networking.Java.Packets.Play
{
    public class PlayerBlockPlacementPacket : Packet<PlayerBlockPlacementPacket>
    {
        public BlockCoordinates Location;
        public BlockFace Face;
        public int Hand;
        public Vector3 CursorPosition;

        public PlayerBlockPlacementPacket()
        {
            PacketId = 0x29;
        }

        public override void Decode(MinecraftStream stream)
        {
            throw new NotImplementedException();
        }

        public override void Encode(MinecraftStream stream)
        {
            stream.WritePosition(Location);
            switch (Face)
            {
                case BlockFace.Down:
                    stream.WriteVarInt(0);
                    break;
                case BlockFace.Up:
                    stream.WriteVarInt(1);
                    break;
                case BlockFace.North:
                    stream.WriteVarInt(2);
                    break;
                case BlockFace.South:
                    stream.WriteVarInt(3);
                    break;
                case BlockFace.West:
                    stream.WriteVarInt(4);
                    break;
                case BlockFace.East:
                    stream.WriteVarInt(5);
                    break;
                case BlockFace.None:
                    stream.WriteVarInt(1);
                    break;
            }

            stream.WriteVarInt(Hand);
            stream.WriteFloat(CursorPosition.X);
            stream.WriteFloat(CursorPosition.Y);
            stream.WriteFloat(CursorPosition.Z);
        }
    }
}
