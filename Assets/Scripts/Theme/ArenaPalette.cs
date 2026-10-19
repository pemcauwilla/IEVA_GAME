using System;
using UnityEngine;

[Serializable]
public struct ArenaPalette
{
    public string PaletteName;
    public Color VoidGroundColor;
    public Color VoidWallColor;
    public Color NeutralZoneColor;
    public Color Player1Color;
    public Color Player2Color;
    public Color GuardianPatrolColor;
    public Color GuardianAlertColor;
    public Color GuardianChaseColor;
    public Color TotemColor;
    public Color GateStripColor;

    public static ArenaPalette[] GetDefaultPalettes()
    {
        return new ArenaPalette[]
        {
            new ArenaPalette
            {
                PaletteName = "CYBER-SYNTH",
                VoidGroundColor = new Color(0.04f, 0.05f, 0.075f, 1f),
                VoidWallColor = new Color(0.11f, 0.13f, 0.18f, 1f),
                NeutralZoneColor = new Color(0.32f, 0.36f, 0.44f, 1f),
                Player1Color = new Color(0.0f, 0.92f, 1.0f, 1f),
                Player2Color = new Color(1.0f, 0.12f, 0.62f, 1f),
                GuardianPatrolColor = new Color(0.42f, 0.35f, 0.78f, 1f),
                GuardianAlertColor = new Color(1.0f, 0.88f, 0.15f, 1f),
                GuardianChaseColor = new Color(1.0f, 0.15f, 0.20f, 1f),
                TotemColor = new Color(0.15f, 1.0f, 0.58f, 1f),
                GateStripColor = new Color(1.0f, 0.52f, 0.12f, 1f)
            },

            new ArenaPalette
            {
                PaletteName = "SOLAR-ABYSS",
                VoidGroundColor = new Color(0.045f, 0.04f, 0.065f, 1f),
                VoidWallColor = new Color(0.12f, 0.10f, 0.16f, 1f),
                NeutralZoneColor = new Color(0.35f, 0.33f, 0.40f, 1f),
                Player1Color = new Color(1.0f, 0.72f, 0.08f, 1f),
                Player2Color = new Color(0.62f, 0.20f, 1.0f, 1f),
                GuardianPatrolColor = new Color(0.18f, 0.58f, 0.65f, 1f),
                GuardianAlertColor = new Color(1.0f, 0.95f, 0.40f, 1f),
                GuardianChaseColor = new Color(1.0f, 0.22f, 0.08f, 1f),
                TotemColor = new Color(0.20f, 0.95f, 0.50f, 1f),
                GateStripColor = new Color(0.15f, 0.82f, 0.95f, 1f)
            },

            new ArenaPalette
            {
                PaletteName = "BIO-GLITCH",
                VoidGroundColor = new Color(0.04f, 0.055f, 0.05f, 1f),
                VoidWallColor = new Color(0.10f, 0.14f, 0.12f, 1f),
                NeutralZoneColor = new Color(0.30f, 0.36f, 0.33f, 1f),
                Player1Color = new Color(0.08f, 0.98f, 0.48f, 1f),
                Player2Color = new Color(1.0f, 0.18f, 0.32f, 1f),
                GuardianPatrolColor = new Color(0.25f, 0.48f, 0.88f, 1f),
                GuardianAlertColor = new Color(0.88f, 1.0f, 0.15f, 1f),
                GuardianChaseColor = new Color(1.0f, 0.10f, 0.15f, 1f),
                TotemColor = new Color(0.12f, 0.95f, 0.88f, 1f),
                GateStripColor = new Color(1.0f, 0.68f, 0.12f, 1f)
            },

            new ArenaPalette
            {
                PaletteName = "FROST-EMBER",
                VoidGroundColor = new Color(0.035f, 0.05f, 0.07f, 1f),
                VoidWallColor = new Color(0.11f, 0.14f, 0.19f, 1f),
                NeutralZoneColor = new Color(0.34f, 0.38f, 0.45f, 1f),
                Player1Color = new Color(0.20f, 0.68f, 1.0f, 1f),
                Player2Color = new Color(1.0f, 0.38f, 0.06f, 1f),
                GuardianPatrolColor = new Color(0.55f, 0.30f, 0.82f, 1f),
                GuardianAlertColor = new Color(1.0f, 0.90f, 0.20f, 1f),
                GuardianChaseColor = new Color(1.0f, 0.12f, 0.18f, 1f),
                TotemColor = new Color(0.22f, 0.98f, 0.52f, 1f),
                GateStripColor = new Color(0.82f, 0.32f, 1.0f, 1f)
            }
        };
    }
}

