using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public enum Packet {
        ServerIdentification = 0x00,
        Ping = 0x01,
        LevelInitialize = 0x02,
        LevelDataChunk = 0x03,
        LevelFinalize = 0x04,
        SetBlock = 0x06,
        SpawnPlayer = 0x07,
        PositionOrientationTeleport = 0x08,
        PositionOrientationUpdate = 0x09,
        PositionUpdate = 0x0a,
        OrientationUpdate = 0x0b,
        DespawnPlayer = 0x0c,
        Message = 0x0d,
        DisconnectPlayer = 0x0e,
        UpdateUserType = 0x0f,

        // CPE

        ExtInfo = 0x10,
        ExtEntry = 0x11
    }
}
