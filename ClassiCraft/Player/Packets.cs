using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public enum Packet {
        // Standard Protocol
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

        // Protocol Extension
        ExtInfo = 0x10,
        ExtEntry = 0x11,
        ClickDistance = 0x12,
        CustomBlockSupportLevel = 0x13,
        HeldBlock = 0x14,
        SetTextHotKey = 0x15,
        ExtAddPlayerName = 0x16,
        ExtAddEntity = 0x17,
        ExtRemovePlayerName = 0x18,
        EnvColors = 0x19,
        MakeSelection = 0x1A,
        RemoveSelection = 0x1B,
        BlockPermissions = 0x1C,
        ChangeModel = 0x1D,
        EnvMapAppearance = 0x1E,
        EnvSetWeatherType = 0x1F,
        HackControl = 0x20
    }
}
