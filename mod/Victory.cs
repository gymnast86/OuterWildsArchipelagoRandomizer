﻿using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using HarmonyLib;
using System;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
internal class Victory
{
    enum GoalSetting: long
    {
        SongOfFive = 0,
        SongOfSix = 1,
    }

    private static GoalSetting goalSetting = GoalSetting.SongOfFive;

    public static void SetGoal(long goal)
    {
        Randomizer.OWMLModConsole.WriteLine($"SetGoal() called with: {goal}");

        if (Enum.IsDefined(typeof(GoalSetting), goal))
            goalSetting = (GoalSetting)goal;
        else
            Randomizer.OWMLModConsole.WriteLine($"{goal} is not a valid goal setting", OWML.Common.MessageType.Error);
    }
    public static void Setup()
    {
        LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
        {
            if (loadScene != OWScene.EyeOfTheUniverse) return;

            var metSolanum = PlayerData.GetPersistentCondition("MET_SOLANUM");
            var metPrisoner = PlayerData.GetPersistentCondition("MET_PRISONER");

            Randomizer.OWMLModConsole.WriteLine($"EyeOfTheUniverse scene loaded.\n" +
                $"MET_SOLANUM: {metSolanum}\n" +
                $"MET_PRISONER: {metPrisoner}\n" +
                $"Goal setting is: {goalSetting}");

            bool isVictory = false;
            if (goalSetting == GoalSetting.SongOfFive)
                isVictory = true;
            else // currently SongOfSix is the only other goal
            {
                if (metSolanum)
                    isVictory = true;
                else
                {
                    Randomizer.OWMLModConsole.WriteLine($"Goal {goalSetting} is NOT completed. Notifying the player.", OWML.Common.MessageType.Info);
                    Randomizer.InGameAPConsole.AddText("<color=red>Goal NOT completed.</color> Your goal is Song of Six, but you haven't met Solanum yet. " +
                        "You can quickly return to the solar system without completing the Eye by pausing and selecting 'Quit and Reset to Solar System'.");
                }
            }

            if (isVictory)
            {
                Randomizer.OWMLModConsole.WriteLine($"Goal {goalSetting} completed! Notifying AP server.", OWML.Common.MessageType.Success);

                var statusUpdatePacket = new StatusUpdatePacket();
                statusUpdatePacket.Status = ArchipelagoClientState.ClientGoal;
                Randomizer.APSession.Socket.SendPacket(statusUpdatePacket);
            }
        };
    }
}
