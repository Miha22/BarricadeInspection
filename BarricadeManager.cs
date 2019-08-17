using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace practise
{
    public class BarricadeManager : SteamCaller
    {
        private static Collider[] checkColliders = new Collider[2];
        public static readonly byte SAVEDATA_VERSION = 14;
        public static readonly byte BARRICADE_REGIONS = 2;
        public static byte version = BarricadeManager.SAVEDATA_VERSION;
        private static List<Interactable2SalvageBarricade> workingSalvageArray = new List<Interactable2SalvageBarricade>();
        public static DeployBarricadeRequestHandler onDeployBarricadeRequested;
        public static SalvageBarricadeRequestHandler onSalvageBarricadeRequested;
        public static DamageBarricadeRequestHandler onDamageBarricadeRequested;
        public static SalvageBarricadeRequestHandler onHarvestPlantRequested;
        public static BarricadeSpawnedHandler onBarricadeSpawned;
        public static ModifySignRequestHandler onModifySignRequested;
        public static OpenStorageRequestHandler onOpenStorageRequested;
        private static BarricadeManager manager;
        private static List<Collider> barricadeColliders;
        private static List<Collider> vehicleColliders;
        private static List<Collider> vehicleSubColliders;
        private static uint instanceCount;
        private static uint serverActiveDate;

        public static BarricadeManager instance
        {
            get
            {
                return BarricadeManager.manager;
            }
        }

        public static BarricadeRegion[,] regions { get; private set; }

        public static BarricadeRegion[,] BarricadeRegions
        {
            get
            {
                return BarricadeManager.regions;
            }
            set
            {
                BarricadeManager.regions = value;
            }
        }
        public static void load(string path)
        {
            bool flag = false;
            //if (LevelSavedata.fileExists("/Barricades.dat") && Level.info.type == ELevelType.SURVIVAL)
            //{
            River river = new River(path, true, false, true);
            BarricadeManager.version = river.readByte();
            BarricadeManager.serverActiveDate = river.readUInt32();
            if (BarricadeManager.version > (byte)0)
            {
                for (byte index1 = 0; (int)index1 < (int)Regions.WORLD_SIZE; ++index1)
                {
                    for (byte index2 = 0; (int)index2 < (int)Regions.WORLD_SIZE; ++index2)
                    {
                        BarricadeRegion region = BarricadeManager.regions[(int)index1, (int)index2];
                        //BarricadeManager.loadRegion(BarricadeManager.version, river, region);
                    }
                }
                if (BarricadeManager.version > (byte)1)
                {
                    if (BarricadeManager.version > (byte)13)
                    {
                        ushort num = river.readUInt16();
                        for (ushort index = 0; (int)index < (int)num; ++index)
                        {
                            uint instanceID = river.readUInt32();
                            BarricadeRegion region = BarricadeManager.getRegionFromVehicle(VehicleManager.getVehicle(instanceID));
                            if (region == null)
                            {
                                CommandWindow.LogWarning((object)string.Format("Barricades associated with missing vehicle instance ID '{0}' were lost", (object)instanceID));
                                region = BarricadeManager.regions[0, 0];
                            }
                            //BarricadeManager.loadRegion(BarricadeManager.version, river, region);
                        }
                    }
                    else
                    {
                        ushort num = (ushort)Mathf.Min((int)river.readUInt16(), BarricadeManager.plants.Count);
                        for (int index = 0; index < (int)num; ++index)
                        {
                            BarricadeRegion plant = BarricadeManager.plants[index];
                            //BarricadeManager.loadRegion(BarricadeManager.version, river, plant);
                        }
                    }
                }
            }

            //if (BarricadeManager.version < (byte)11)
            //    flag = true;
            //}
            //else
            //    flag = true;
            //if (flag && LevelObjects.buildables != null)
            //{
            //    for (byte index1 = 0; (int)index1 < (int)Regions.WORLD_SIZE; ++index1)
            //    {
            //        for (byte index2 = 0; (int)index2 < (int)Regions.WORLD_SIZE; ++index2)
            //        {
            //            List<LevelBuildableObject> buildable = LevelObjects.buildables[(int)index1, (int)index2];
            //            if (buildable != null && buildable.Count != 0)
            //            {
            //                BarricadeRegion region = BarricadeManager.regions[(int)index1, (int)index2];
            //                for (int index3 = 0; index3 < buildable.Count; ++index3)
            //                {
            //                    LevelBuildableObject levelBuildableObject = buildable[index3];
            //                    if (levelBuildableObject != null)
            //                    {
            //                        ItemBarricadeAsset asset = levelBuildableObject.asset as ItemBarricadeAsset;
            //                        if (asset != null)
            //                        {
            //                            Vector3 eulerAngles = levelBuildableObject.rotation.eulerAngles;
            //                            byte newAngle_X = MeasurementTool.angleToByte((float)(Mathf.RoundToInt(eulerAngles.x / 2f) * 2));
            //                            byte newAngle_Y = MeasurementTool.angleToByte((float)(Mathf.RoundToInt(eulerAngles.y / 2f) * 2));
            //                            byte newAngle_Z = MeasurementTool.angleToByte((float)(Mathf.RoundToInt(eulerAngles.z / 2f) * 2));
            //                            Barricade newBarricade = new Barricade(asset.id, asset.health, asset.getState(), asset);
            //                            BarricadeData barricadeData = new BarricadeData(newBarricade, levelBuildableObject.point, newAngle_X, newAngle_Y, newAngle_Z, 0UL, 0UL, uint.MaxValue);
            //                            region.barricades.Add(barricadeData);
            //                            //BarricadeManager.manager.spawnBarricade(region, newBarricade.id, newBarricade.state, barricadeData.point, barricadeData.angle_x, barricadeData.angle_y, barricadeData.angle_z, (byte)Mathf.RoundToInt((float)((double)newBarricade.health / (double)asset.health * 100.0)), 0UL, 0UL, ++BarricadeManager.instanceCount);
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
            Level.isLoadingBarricades = false;
        }
        public static void loadRegion(River river)
        {
            Console.WriteLine("begin");
            ushort num1 = river.readUInt16();
            for (ushort index = 0; (int)index < (int)num1; ++index)
            {
                ushort num2 = river.readUInt16();
                Console.WriteLine();
                Console.WriteLine($"barricade ID: {num2}");
                ushort newHealth = river.readUInt16();
                byte[] numArray = river.readBytes();
                foreach (var item in numArray)
                {
                    Console.WriteLine($"state: {item}");
                }
                Console.WriteLine();
            Console.WriteLine("end");
            Vector3 vector3 = river.readSingleVector3();
            byte num3 = 0;
            if (version > (byte)2)
                num3 = river.readByte();
            byte num4 = river.readByte();
            byte num5 = 0;
            if (version > (byte)3)
                num5 = river.readByte();
            ulong num6 = 0;
            ulong num7 = 0;
            if (version > (byte)4)
            {
                num6 = river.readUInt64();
                num7 = river.readUInt64();
            }
            uint newObjActiveDate = river.readUInt32();
                newObjActiveDate = 123123213;
            //ItemBarricadeAsset itemBarricadeAsset;
            
            
            }
        }
        public static void save(string path)
        {
            River river = new River(path, true, false, true);
            river.writeByte(BarricadeManager.SAVEDATA_VERSION);
            river.writeUInt32(Provider.time);
            for (byte index1 = 0; (int)index1 < (int)Regions.WORLD_SIZE; ++index1)
            {
                for (byte index2 = 0; (int)index2 < (int)Regions.WORLD_SIZE; ++index2)
                {
                    BarricadeRegion region = BarricadeManager.regions[(int)index1, (int)index2];
                    BarricadeManager.saveRegion(river, region);
                }
            }
            ushort num = 0;
            for (ushort plant = 0; (int)plant < BarricadeManager.plants.Count; ++plant)
            {
                InteractableVehicle vehicleFromPlant = BarricadeManager.getVehicleFromPlant(plant);
                if ((UnityEngine.Object)vehicleFromPlant != (UnityEngine.Object)null && !vehicleFromPlant.isAutoClearable)
                    ++num;
            }
            river.writeUInt16(num);
            for (int index = 0; index < BarricadeManager.plants.Count; ++index)
            {
                InteractableVehicle vehicleFromPlant = BarricadeManager.getVehicleFromPlant((ushort)index);
                if ((UnityEngine.Object)vehicleFromPlant != (UnityEngine.Object)null && !vehicleFromPlant.isAutoClearable)
                {
                    river.writeUInt32(vehicleFromPlant.instanceID);
                    BarricadeManager.saveRegion(river, BarricadeManager.plants[index]);
                }
            }
            river.closeRiver();
        }
        private static void saveRegion(River river, BarricadeRegion region)
        {
            uint time = Provider.time;
            ushort num = 0;
            for (ushort index = 0; (int)index < region.barricades.Count; ++index)
            {
                BarricadeData barricade = region.barricades[(int)index];
                if ((!Dedicator.isDedicated || Provider.modeConfigData.Barricades.Decay_Time == 0U || (time < barricade.objActiveDate || time - barricade.objActiveDate < Provider.modeConfigData.Barricades.Decay_Time)) && barricade.barricade.asset.isSaveable)
                    ++num;
            }
            river.writeUInt16(num);
            for (ushort index = 0; (int)index < region.barricades.Count; ++index)
            {
                BarricadeData barricade = region.barricades[(int)index];
                river.writeUInt16(barricade.barricade.id);
                river.writeUInt16(barricade.barricade.health);
                river.writeBytes(barricade.barricade.state);
                river.writeSingleVector3(barricade.point);
                river.writeByte(barricade.angle_x);
                river.writeByte(barricade.angle_y);
                river.writeByte(barricade.angle_z);
                river.writeUInt64(barricade.owner);
                river.writeUInt64(barricade.group);
                river.writeUInt32(barricade.objActiveDate);
                //if ((!Dedicator.isDedicated || Provider.modeConfigData.Barricades.Decay_Time == 0U || (time < barricade.objActiveDate || time - barricade.objActiveDate < Provider.modeConfigData.Barricades.Decay_Time)) && barricade.barricade.asset.isSaveable)
                //{
                //    river.writeUInt16(barricade.barricade.id);
                //    river.writeUInt16(barricade.barricade.health);
                //    river.writeBytes(barricade.barricade.state);
                //    river.writeSingleVector3(barricade.point);
                //    river.writeByte(barricade.angle_x);
                //    river.writeByte(barricade.angle_y);
                //    river.writeByte(barricade.angle_z);
                //    river.writeUInt64(barricade.owner);
                //    river.writeUInt64(barricade.group);
                //    river.writeUInt32(barricade.objActiveDate);
                //}
            }
        }
        public static List<BarricadeRegion> plants { get; private set; }

        private static void getVehicleColliders(Transform vehicleRoot)
        {
            BarricadeManager.vehicleColliders.Clear();
            vehicleRoot.GetComponents<Collider>(BarricadeManager.vehicleColliders);
            BarricadeManager.recursivelyAddChildAndBlockColliders(vehicleRoot);
        }

        private static void recursivelyAddChildAndBlockColliders(Transform parent)
        {
            for (int index = 0; index < parent.childCount; ++index)
            {
                Transform child = parent.GetChild(index);
                if (!((UnityEngine.Object)child == (UnityEngine.Object)null))
                {
                    if (child.name == "Clip" || child.name == "Block")
                    {
                        BarricadeManager.vehicleSubColliders.Clear();
                        child.GetComponents<Collider>(BarricadeManager.vehicleSubColliders);
                        using (List<Collider>.Enumerator enumerator = BarricadeManager.vehicleSubColliders.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                Collider current = enumerator.Current;
                                BarricadeManager.vehicleColliders.Add(current);
                            }
                        }
                    }
                    BarricadeManager.recursivelyAddChildAndBlockColliders(child);
                }
            }
        }

        public static void getBarricadesInRadius(
          Vector3 center,
          float sqrRadius,
          List<RegionCoordinate> search,
          List<Transform> result)
        {
            if (BarricadeManager.regions == null)
                return;
            for (int index1 = 0; index1 < search.Count; ++index1)
            {
                RegionCoordinate regionCoordinate = search[index1];
                if (BarricadeManager.regions[(int)regionCoordinate.x, (int)regionCoordinate.y] != null)
                {
                    for (int index2 = 0; index2 < BarricadeManager.regions[(int)regionCoordinate.x, (int)regionCoordinate.y].drops.Count; ++index2)
                    {
                        Transform model = BarricadeManager.regions[(int)regionCoordinate.x, (int)regionCoordinate.y].drops[index2].model;
                        if ((double)(model.position - center).sqrMagnitude < (double)sqrRadius)
                            result.Add(model);
                    }
                }
            }
        }

        public static void getBarricadesInRadius(
          Vector3 center,
          float sqrRadius,
          ushort plant,
          List<Transform> result)
        {
            if (BarricadeManager.plants == null || (int)plant >= BarricadeManager.plants.Count)
                return;
            for (int index = 0; index < BarricadeManager.plants[(int)plant].drops.Count; ++index)
            {
                Transform model = BarricadeManager.plants[(int)plant].drops[index].model;
                if ((double)(model.position - center).sqrMagnitude < (double)sqrRadius)
                    result.Add(model);
            }
        }

        public static void getBarricadesInRadius(
          Vector3 center,
          float sqrRadius,
          List<Transform> result)
        {
            if (BarricadeManager.plants == null)
                return;
            for (ushort plant = 0; (int)plant < BarricadeManager.plants.Count; ++plant)
                BarricadeManager.getBarricadesInRadius(center, sqrRadius, plant, result);
        }

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public void tellBarricadeOwnerAndGroup(
          CSteamID steamID,
          byte x,
          byte y,
          ushort plant,
          ushort index,
          ulong newOwner,
          ulong newGroup)
        {
            BarricadeRegion region;
            if (!BarricadeManager.tryGetRegion(x, y, plant, out region) || !Provider.isServer && !region.isNetworked || (int)index >= region.drops.Count)
                return;
            BarricadeManager.workingSalvageArray.Clear();
            region.drops[(int)index].model.GetComponentsInChildren<Interactable2SalvageBarricade>(BarricadeManager.workingSalvageArray);
            foreach (Interactable2SalvageBarricade workingSalvage in BarricadeManager.workingSalvageArray)
            {
                workingSalvage.owner = newOwner;
                workingSalvage.group = newGroup;
            }
        }

        //public static void changeOwnerAndGroup(Transform barricade, ulong newOwner, ulong newGroup)
        //{
        //    byte x;
        //    byte y;
        //    ushort plant;
        //    ushort index;
        //    BarricadeRegion region;
        //    if (!BarricadeManager.tryGetInfo(barricade, out x, out y, out plant, out index, out region))
        //        return;
        //    if (plant == ushort.MaxValue)
        //        BarricadeManager.manager.channel.send("tellBarricadeOwnerAndGroup", ESteamCall.ALL, x, y, BarricadeManager.BARRICADE_REGIONS, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)newOwner, (object)newGroup);
        //    else
        //        BarricadeManager.manager.channel.send("tellBarricadeOwnerAndGroup", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)newOwner, (object)newGroup);
        //    region.barricades[(int)index].owner = newOwner;
        //    region.barricades[(int)index].group = newGroup;
        //    BarricadeManager.sendHealthChanged(x, y, plant, index, region);
        //}

        public static void transformBarricade(
          Transform barricade,
          Vector3 point,
          float angle_x,
          float angle_y,
          float angle_z)
        {
            angle_x = (float)(Mathf.RoundToInt(angle_x / 2f) * 2);
            angle_y = (float)(Mathf.RoundToInt(angle_y / 2f) * 2);
            angle_z = (float)(Mathf.RoundToInt(angle_z / 2f) * 2);
            byte x;
            byte y;
            ushort plant;
            ushort index;
            BarricadeRegion region;
            BarricadeDrop drop;
            if (!BarricadeManager.tryGetInfo(barricade, out x, out y, out plant, out index, out region, out drop))
                return;
            BarricadeManager.manager.channel.send("askTransformBarricade", ESteamCall.SERVER, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)drop.instanceID, (object)point, (object)MeasurementTool.angleToByte(angle_x), (object)MeasurementTool.angleToByte(angle_y), (object)MeasurementTool.angleToByte(angle_z));
        }

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public void tellTransformBarricade(
          CSteamID steamID,
          byte x,
          byte y,
          ushort plant,
          uint instanceID,
          Vector3 point,
          byte angle_x,
          byte angle_y,
          byte angle_z)
        {
            BarricadeRegion region1;
            if (!this.channel.checkServer(steamID) || !BarricadeManager.tryGetRegion(x, y, plant, out region1) || !Provider.isServer && !region1.isNetworked)
                return;
            BarricadeData barricadeData = (BarricadeData)null;
            BarricadeDrop barricadeDrop = (BarricadeDrop)null;
            ushort num;
            for (num = (ushort)0; (int)num < region1.drops.Count; ++num)
            {
                if ((int)region1.drops[(int)num].instanceID == (int)instanceID)
                {
                    if (Provider.isServer)
                        barricadeData = region1.barricades[(int)num];
                    barricadeDrop = region1.drops[(int)num];
                    break;
                }
            }
            if (barricadeDrop == null)
                return;
            barricadeDrop.model.position = point;
            barricadeDrop.model.rotation = Quaternion.Euler((float)((int)angle_x * 2), (float)((int)angle_y * 2), (float)((int)angle_z * 2));
            if (plant == ushort.MaxValue)
            {
                byte x1;
                byte y1;
                if (Regions.tryGetCoordinate(point, out x1, out y1) && ((int)x != (int)x1 || (int)y != (int)y1))
                {
                    BarricadeRegion region2 = BarricadeManager.regions[(int)x1, (int)y1];
                    region1.drops.RemoveAt((int)num);
                    if (region2.isNetworked || Provider.isServer)
                        region2.drops.Add(barricadeDrop);
                    else if (!Provider.isServer)
                        UnityEngine.Object.Destroy((UnityEngine.Object)barricadeDrop.model.gameObject);
                    if (Provider.isServer)
                    {
                        region1.barricades.RemoveAt((int)num);
                        region2.barricades.Add(barricadeData);
                    }
                }
                if (!Provider.isServer)
                    return;
                barricadeData.point = point;
                barricadeData.angle_x = angle_x;
                barricadeData.angle_y = angle_y;
                barricadeData.angle_z = angle_z;
            }
            else
            {
                if (!Provider.isServer)
                    return;
                barricadeData.point = barricadeDrop.model.localPosition;
                Vector3 eulerAngles = barricadeDrop.model.localRotation.eulerAngles;
                barricadeData.angle_x = MeasurementTool.angleToByte((float)(Mathf.RoundToInt(eulerAngles.x / 2f) * 2));
                barricadeData.angle_y = MeasurementTool.angleToByte((float)(Mathf.RoundToInt(eulerAngles.y / 2f) * 2));
                barricadeData.angle_z = MeasurementTool.angleToByte((float)(Mathf.RoundToInt(eulerAngles.z / 2f) * 2));
            }
        }

        [SteamCall(ESteamCallValidation.SERVERSIDE)]
        public void askTransformBarricade(
          CSteamID steamID,
          byte x,
          byte y,
          ushort plant,
          uint instanceID,
          Vector3 point,
          byte angle_x,
          byte angle_y,
          byte angle_z)
        {
            BarricadeRegion region;
            if (!Provider.isServer || !BarricadeManager.tryGetRegion(x, y, plant, out region))
                return;
            Player player = PlayerTool.getPlayer(steamID);
            if ((UnityEngine.Object)player == (UnityEngine.Object)null || player.life.isDead || !player.look.canUseWorkzone)
                return;
            if (plant == ushort.MaxValue)
                BarricadeManager.manager.channel.send("tellTransformBarricade", ESteamCall.ALL, x, y, BarricadeManager.BARRICADE_REGIONS, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)instanceID, (object)point, (object)angle_x, (object)angle_y, (object)angle_z);
            else
                BarricadeManager.manager.channel.send("tellTransformBarricade", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)instanceID, (object)point, (object)angle_x, (object)angle_y, (object)angle_z);
        }

        public static void poseMannequin(Transform barricade, byte poseComp)
        {
            byte x;
            byte y;
            ushort plant;
            ushort index;
            BarricadeRegion region;
            if (!BarricadeManager.tryGetInfo(barricade, out x, out y, out plant, out index, out region))
                return;
            BarricadeManager.manager.channel.send("askPoseMannequin", ESteamCall.SERVER, ESteamPacket.UPDATE_UNRELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)poseComp);
        }

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public void tellPoseMannequin(
          CSteamID steamID,
          byte x,
          byte y,
          ushort plant,
          ushort index,
          byte poseComp)
        {
            BarricadeRegion region;
            if (!this.channel.checkServer(steamID) || !BarricadeManager.tryGetRegion(x, y, plant, out region) || !Provider.isServer && !region.isNetworked || (int)index >= region.drops.Count)
                return;
            InteractableMannequin interactable = region.drops[(int)index].interactable as InteractableMannequin;
            if (!((UnityEngine.Object)interactable != (UnityEngine.Object)null))
                return;
            interactable.setPose(poseComp);
        }

        [SteamCall(ESteamCallValidation.SERVERSIDE)]
        public void askPoseMannequin(
          CSteamID steamID,
          byte x,
          byte y,
          ushort plant,
          ushort index,
          byte poseComp)
        {
            BarricadeRegion region;
            if (!Provider.isServer || !BarricadeManager.tryGetRegion(x, y, plant, out region))
                return;
            Player player = PlayerTool.getPlayer(steamID);
            if ((UnityEngine.Object)player == (UnityEngine.Object)null || player.life.isDead || ((int)index >= region.drops.Count || !player.tryToPerformRateLimitedAction()) || (double)(region.drops[(int)index].model.transform.position - player.transform.position).sqrMagnitude > 400.0)
                return;
            InteractableMannequin interactable = region.drops[(int)index].interactable as InteractableMannequin;
            if (!((UnityEngine.Object)interactable != (UnityEngine.Object)null) || !interactable.checkUpdate(player.channel.owner.playerID.steamID, player.quests.groupID))
                return;
            if (plant == ushort.MaxValue)
                BarricadeManager.manager.channel.send("tellPoseMannequin", ESteamCall.ALL, x, y, BarricadeManager.BARRICADE_REGIONS, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)poseComp);
            else
                BarricadeManager.manager.channel.send("tellPoseMannequin", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)poseComp);
            interactable.rebuildState();
        }

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public void tellUpdateMannequin(
          CSteamID steamID,
          byte x,
          byte y,
          ushort plant,
          ushort index,
          byte[] state)
        {
            BarricadeRegion region;
            if (!this.channel.checkServer(steamID) || !BarricadeManager.tryGetRegion(x, y, plant, out region) || !Provider.isServer && !region.isNetworked || (int)index >= region.drops.Count)
                return;
            InteractableMannequin interactable = region.drops[(int)index].interactable as InteractableMannequin;
            if (!((UnityEngine.Object)interactable != (UnityEngine.Object)null))
                return;
            interactable.updateState(state);
        }

        public static void updateMannequin(Transform barricade, EMannequinUpdateMode updateMode)
        {
            byte x;
            byte y;
            ushort plant;
            ushort index;
            BarricadeRegion region;
            if (!BarricadeManager.tryGetInfo(barricade, out x, out y, out plant, out index, out region))
                return;
            BarricadeManager.manager.channel.send("askUpdateMannequin", ESteamCall.SERVER, ESteamPacket.UPDATE_UNRELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)(byte)updateMode);
        }

        [SteamCall(ESteamCallValidation.SERVERSIDE)]
        public void askUpdateMannequin(
          CSteamID steamID,
          byte x,
          byte y,
          ushort plant,
          ushort index,
          byte mode)
        {
            BarricadeRegion region;
            if (!Provider.isServer || !BarricadeManager.tryGetRegion(x, y, plant, out region))
                return;
            Player player = PlayerTool.getPlayer(steamID);
            if ((UnityEngine.Object)player == (UnityEngine.Object)null || player.life.isDead || player.equipment.isBusy || (player.equipment.isSelected && !player.equipment.isEquipped || ((int)index >= region.drops.Count || !player.tryToPerformRateLimitedAction())) || (double)(region.drops[(int)index].model.transform.position - player.transform.position).sqrMagnitude > 400.0)
                return;
            InteractableMannequin interactable = region.drops[(int)index].interactable as InteractableMannequin;
            if (!((UnityEngine.Object)interactable != (UnityEngine.Object)null) || !interactable.isUpdatable || !interactable.checkUpdate(player.channel.owner.playerID.steamID, player.quests.groupID))
                return;
            switch (mode)
            {
                case 0:
                    interactable.updateVisuals(player.clothing.visualShirt, player.clothing.visualPants, player.clothing.visualHat, player.clothing.visualBackpack, player.clothing.visualVest, player.clothing.visualMask, player.clothing.visualGlasses);
                    if (interactable.shirt != (ushort)0)
                        player.inventory.forceAddItem(new Item(interactable.shirt, (byte)1, interactable.shirtQuality, interactable.shirtState), false);
                    if (interactable.pants != (ushort)0)
                        player.inventory.forceAddItem(new Item(interactable.pants, (byte)1, interactable.pantsQuality, interactable.pantsState), false);
                    if (interactable.hat != (ushort)0)
                        player.inventory.forceAddItem(new Item(interactable.hat, (byte)1, interactable.hatQuality, interactable.hatState), false);
                    if (interactable.backpack != (ushort)0)
                        player.inventory.forceAddItem(new Item(interactable.backpack, (byte)1, interactable.backpackQuality, interactable.backpackState), false);
                    if (interactable.vest != (ushort)0)
                        player.inventory.forceAddItem(new Item(interactable.vest, (byte)1, interactable.vestQuality, interactable.vestState), false);
                    if (interactable.mask != (ushort)0)
                        player.inventory.forceAddItem(new Item(interactable.mask, (byte)1, interactable.maskQuality, interactable.maskState), false);
                    if (interactable.glasses != (ushort)0)
                        player.inventory.forceAddItem(new Item(interactable.glasses, (byte)1, interactable.glassesQuality, interactable.glassesState), false);
                    interactable.clearClothes();
                    break;
                case 1:
                    if (!player.equipment.isSelected || !player.equipment.isEquipped || (player.equipment.isBusy || player.equipment.asset == null) || !(player.equipment.useable is UseableClothing))
                        return;
                    ItemJar itemJar = player.inventory.getItem(player.equipment.equippedPage, player.inventory.getIndex(player.equipment.equippedPage, player.equipment.equipped_x, player.equipment.equipped_y));
                    if (itemJar == null || itemJar.item == null)
                        return;
                    interactable.clearVisuals();
                    switch (player.equipment.asset.type)
                    {
                        case EItemType.HAT:
                            if (interactable.hat != (ushort)0)
                                player.inventory.forceAddItem(new Item(interactable.hat, (byte)1, interactable.hatQuality, interactable.hatState), false);
                            interactable.clothes.hat = itemJar.item.id;
                            interactable.hatQuality = itemJar.item.quality;
                            interactable.hatState = itemJar.item.state;
                            break;
                        case EItemType.PANTS:
                            if (interactable.pants != (ushort)0)
                                player.inventory.forceAddItem(new Item(interactable.pants, (byte)1, interactable.pantsQuality, interactable.pantsState), false);
                            interactable.clothes.pants = itemJar.item.id;
                            interactable.pantsQuality = itemJar.item.quality;
                            interactable.pantsState = itemJar.item.state;
                            break;
                        case EItemType.SHIRT:
                            if (interactable.shirt != (ushort)0)
                                player.inventory.forceAddItem(new Item(interactable.shirt, (byte)1, interactable.shirtQuality, interactable.shirtState), false);
                            interactable.clothes.shirt = itemJar.item.id;
                            interactable.shirtQuality = itemJar.item.quality;
                            interactable.shirtState = itemJar.item.state;
                            break;
                        case EItemType.MASK:
                            if (interactable.mask != (ushort)0)
                                player.inventory.forceAddItem(new Item(interactable.mask, (byte)1, interactable.maskQuality, interactable.maskState), false);
                            interactable.clothes.mask = itemJar.item.id;
                            interactable.maskQuality = itemJar.item.quality;
                            interactable.maskState = itemJar.item.state;
                            break;
                        case EItemType.BACKPACK:
                            if (interactable.backpack != (ushort)0)
                                player.inventory.forceAddItem(new Item(interactable.backpack, (byte)1, interactable.backpackQuality, interactable.backpackState), false);
                            interactable.clothes.backpack = itemJar.item.id;
                            interactable.backpackQuality = itemJar.item.quality;
                            interactable.backpackState = itemJar.item.state;
                            break;
                        case EItemType.VEST:
                            if (interactable.vest != (ushort)0)
                                player.inventory.forceAddItem(new Item(interactable.vest, (byte)1, interactable.vestQuality, interactable.vestState), false);
                            interactable.clothes.vest = itemJar.item.id;
                            interactable.vestQuality = itemJar.item.quality;
                            interactable.vestState = itemJar.item.state;
                            break;
                        case EItemType.GLASSES:
                            if (interactable.glasses != (ushort)0)
                                player.inventory.forceAddItem(new Item(interactable.glasses, (byte)1, interactable.glassesQuality, interactable.glassesState), false);
                            interactable.clothes.glasses = itemJar.item.id;
                            interactable.glassesQuality = itemJar.item.quality;
                            interactable.glassesState = itemJar.item.state;
                            break;
                        default:
                            return;
                    }
                    player.equipment.use();
                    break;
                case 2:
                    interactable.clearVisuals();
                    if (interactable.shirt != (ushort)0)
                        player.inventory.forceAddItem(new Item(interactable.shirt, (byte)1, interactable.shirtQuality, interactable.shirtState), true, false);
                    if (interactable.pants != (ushort)0)
                        player.inventory.forceAddItem(new Item(interactable.pants, (byte)1, interactable.pantsQuality, interactable.pantsState), true, false);
                    if (interactable.hat != (ushort)0)
                        player.inventory.forceAddItem(new Item(interactable.hat, (byte)1, interactable.hatQuality, interactable.hatState), true, false);
                    if (interactable.backpack != (ushort)0)
                        player.inventory.forceAddItem(new Item(interactable.backpack, (byte)1, interactable.backpackQuality, interactable.backpackState), true, false);
                    if (interactable.vest != (ushort)0)
                        player.inventory.forceAddItem(new Item(interactable.vest, (byte)1, interactable.vestQuality, interactable.vestState), true, false);
                    if (interactable.mask != (ushort)0)
                        player.inventory.forceAddItem(new Item(interactable.mask, (byte)1, interactable.maskQuality, interactable.maskState), true, false);
                    if (interactable.glasses != (ushort)0)
                        player.inventory.forceAddItem(new Item(interactable.glasses, (byte)1, interactable.glassesQuality, interactable.glassesState), true, false);
                    interactable.clearClothes();
                    break;
                case 3:
                    interactable.clearVisuals();
                    ushort shirt = player.clothing.shirt;
                    byte shirtQuality = player.clothing.shirtQuality;
                    byte[] shirtState = player.clothing.shirtState;
                    ushort pants = player.clothing.pants;
                    byte pantsQuality = player.clothing.pantsQuality;
                    byte[] pantsState = player.clothing.pantsState;
                    ushort hat = player.clothing.hat;
                    byte hatQuality = player.clothing.hatQuality;
                    byte[] hatState = player.clothing.hatState;
                    ushort backpack = player.clothing.backpack;
                    byte backpackQuality = player.clothing.backpackQuality;
                    byte[] backpackState = player.clothing.backpackState;
                    ushort vest = player.clothing.vest;
                    byte vestQuality = player.clothing.vestQuality;
                    byte[] vestState = player.clothing.vestState;
                    ushort mask = player.clothing.mask;
                    byte maskQuality = player.clothing.maskQuality;
                    byte[] maskState = player.clothing.maskState;
                    ushort glasses = player.clothing.glasses;
                    byte glassesQuality = player.clothing.glassesQuality;
                    byte[] glassesState = player.clothing.glassesState;
                    player.clothing.updateClothes(interactable.shirt, interactable.shirtQuality, interactable.shirtState, interactable.pants, interactable.pantsQuality, interactable.pantsState, interactable.hat, interactable.hatQuality, interactable.hatState, interactable.backpack, interactable.backpackQuality, interactable.backpackState, interactable.vest, interactable.vestQuality, interactable.vestState, interactable.mask, interactable.maskQuality, interactable.maskState, interactable.glasses, interactable.glassesQuality, interactable.glassesState);
                    interactable.updateClothes(shirt, shirtQuality, shirtState, pants, pantsQuality, pantsState, hat, hatQuality, hatState, backpack, backpackQuality, backpackState, vest, vestQuality, vestState, mask, maskQuality, maskState, glasses, glassesQuality, glassesState);
                    break;
                default:
                    return;
            }
            interactable.rebuildState();
            if (plant == ushort.MaxValue)
                BarricadeManager.manager.channel.send("tellUpdateMannequin", ESteamCall.ALL, x, y, BarricadeManager.BARRICADE_REGIONS, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)region.barricades[(int)index].barricade.state);
            else
                BarricadeManager.manager.channel.send("tellUpdateMannequin", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)region.barricades[(int)index].barricade.state);
            EffectManager.sendEffect((ushort)9, EffectManager.SMALL, interactable.transform.position);
        }

        public static void rotDisplay(Transform barricade, byte rotComp)
        {
            byte x;
            byte y;
            ushort plant;
            ushort index;
            BarricadeRegion region;
            if (!BarricadeManager.tryGetInfo(barricade, out x, out y, out plant, out index, out region))
                return;
            BarricadeManager.manager.channel.send("askRotDisplay", ESteamCall.SERVER, ESteamPacket.UPDATE_UNRELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)rotComp);
        }

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public void tellRotDisplay(
          CSteamID steamID,
          byte x,
          byte y,
          ushort plant,
          ushort index,
          byte rotComp)
        {
            BarricadeRegion region;
            if (!this.channel.checkServer(steamID) || !BarricadeManager.tryGetRegion(x, y, plant, out region) || !Provider.isServer && !region.isNetworked || (int)index >= region.drops.Count)
                return;
            InteractableStorage interactable = region.drops[(int)index].interactable as InteractableStorage;
            if (!((UnityEngine.Object)interactable != (UnityEngine.Object)null))
                return;
            interactable.setRotation(rotComp);
        }

        [SteamCall(ESteamCallValidation.SERVERSIDE)]
        public void askRotDisplay(
          CSteamID steamID,
          byte x,
          byte y,
          ushort plant,
          ushort index,
          byte rotComp)
        {
            BarricadeRegion region;
            if (!Provider.isServer || !BarricadeManager.tryGetRegion(x, y, plant, out region))
                return;
            Player player = PlayerTool.getPlayer(steamID);
            if ((UnityEngine.Object)player == (UnityEngine.Object)null || player.life.isDead || ((int)index >= region.drops.Count || !player.tryToPerformRateLimitedAction()) || (double)(region.drops[(int)index].model.transform.position - player.transform.position).sqrMagnitude > 400.0)
                return;
            InteractableStorage interactable = region.drops[(int)index].interactable as InteractableStorage;
            if (!((UnityEngine.Object)interactable != (UnityEngine.Object)null) || !interactable.checkRot(player.channel.owner.playerID.steamID, player.quests.groupID) || !interactable.isDisplay)
                return;
            if (plant == ushort.MaxValue)
                BarricadeManager.manager.channel.send("tellRotDisplay", ESteamCall.ALL, x, y, BarricadeManager.BARRICADE_REGIONS, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)rotComp);
            else
                BarricadeManager.manager.channel.send("tellRotDisplay", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)rotComp);
            interactable.rebuildState();
        }

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public void tellBarricadeHealth(
          CSteamID steamID,
          byte x,
          byte y,
          ushort plant,
          ushort index,
          byte hp)
        {
            BarricadeRegion region;
            if (!this.channel.checkServer(steamID) || !BarricadeManager.tryGetRegion(x, y, plant, out region) || !Provider.isServer && !region.isNetworked || (int)index >= region.drops.Count)
                return;
            Interactable2HP component = region.drops[(int)index].model.GetComponent<Interactable2HP>();
            if (!((UnityEngine.Object)component != (UnityEngine.Object)null))
                return;
            component.hp = hp;
        }

        public static void salvageBarricade(Transform barricade)
        {
            byte x;
            byte y;
            ushort plant;
            ushort index;
            BarricadeRegion region;
            if (!BarricadeManager.tryGetInfo(barricade, out x, out y, out plant, out index, out region))
                return;
            BarricadeManager.manager.channel.send("askSalvageBarricade", ESteamCall.SERVER, ESteamPacket.UPDATE_UNRELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index);
        }

        //[SteamCall(ESteamCallValidation.SERVERSIDE)]
        //public void askSalvageBarricade(CSteamID steamID, byte x, byte y, ushort plant, ushort index)
        //{
        //    BarricadeRegion region;
        //    if (!Provider.isServer || !BarricadeManager.tryGetRegion(x, y, plant, out region))
        //        return;
        //    Player player = PlayerTool.getPlayer(steamID);
        //    if ((UnityEngine.Object)player == (UnityEngine.Object)null || player.life.isDead || ((int)index >= region.drops.Count || !player.tryToPerformRateLimitedAction()) || !OwnershipTool.checkToggle(player.channel.owner.playerID.steamID, region.barricades[(int)index].owner, player.quests.groupID, region.barricades[(int)index].group))
        //        return;
        //    bool shouldAllow = true;
        //    if (BarricadeManager.onSalvageBarricadeRequested != null)
        //        BarricadeManager.onSalvageBarricadeRequested(steamID, x, y, plant, index, ref shouldAllow);
        //    if (!shouldAllow)
        //        return;
        //    ItemBarricadeAsset itemBarricadeAsset = (ItemBarricadeAsset)Assets.find(EAssetType.ITEM, region.barricades[(int)index].barricade.id);
        //    if (itemBarricadeAsset != null)
        //    {
        //        if (itemBarricadeAsset.isUnpickupable)
        //            return;
        //        if ((int)region.barricades[(int)index].barricade.health >= (int)itemBarricadeAsset.health)
        //            player.inventory.forceAddItem(new Item(region.barricades[(int)index].barricade.id, EItemOrigin.NATURE), true);
        //        else if (itemBarricadeAsset.isSalvageable)
        //        {
        //            for (int index1 = 0; index1 < itemBarricadeAsset.blueprints.Count; ++index1)
        //            {
        //                Blueprint blueprint = itemBarricadeAsset.blueprints[index1];
        //                if (blueprint.outputs.Length == 1 && (int)blueprint.outputs[0].id == (int)itemBarricadeAsset.id)
        //                {
        //                    ushort id = blueprint.supplies[UnityEngine.Random.Range(0, blueprint.supplies.Length)].id;
        //                    player.inventory.forceAddItem(new Item(id, EItemOrigin.NATURE), true);
        //                    break;
        //                }
        //            }
        //        }
        //    }
        //    region.barricades.RemoveAt((int)index);
        //    if (plant == ushort.MaxValue)
        //        BarricadeManager.manager.channel.send("tellTakeBarricade", ESteamCall.ALL, x, y, BarricadeManager.BARRICADE_REGIONS, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index);
        //    else
        //        BarricadeManager.manager.channel.send("tellTakeBarricade", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index);
        //}

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public void tellTank(
          CSteamID steamID,
          byte x,
          byte y,
          ushort plant,
          ushort index,
          ushort amount)
        {
            BarricadeRegion region;
            if (!this.channel.checkServer(steamID) || !BarricadeManager.tryGetRegion(x, y, plant, out region) || !Provider.isServer && !region.isNetworked || (int)index >= region.drops.Count)
                return;
            InteractableTank interactable = region.drops[(int)index].interactable as InteractableTank;
            if (!((UnityEngine.Object)interactable != (UnityEngine.Object)null))
                return;
            interactable.updateAmount(amount);
        }

        public static void updateTank(Transform barricade, ushort amount)
        {
            byte x;
            byte y;
            ushort plant;
            ushort index;
            BarricadeRegion region;
            if (!BarricadeManager.tryGetInfo(barricade, out x, out y, out plant, out index, out region))
                return;
            if (plant == ushort.MaxValue)
                BarricadeManager.manager.channel.send("tellTank", ESteamCall.ALL, x, y, BarricadeManager.BARRICADE_REGIONS, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)amount);
            else
                BarricadeManager.manager.channel.send("tellTank", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)amount);
            byte[] bytes = BitConverter.GetBytes(amount);
            region.barricades[(int)index].barricade.state[0] = bytes[0];
            region.barricades[(int)index].barricade.state[1] = bytes[1];
        }

        public static void updateSign(Transform barricade, string newText)
        {
            byte x;
            byte y;
            ushort plant;
            ushort index;
            BarricadeRegion region;
            if (newText.Contains("<size") || !BarricadeManager.tryGetInfo(barricade, out x, out y, out plant, out index, out region))
                return;
            BarricadeManager.manager.channel.send("askUpdateSign", ESteamCall.SERVER, ESteamPacket.UPDATE_UNRELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)newText);
        }

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public void tellUpdateSign(
          CSteamID steamID,
          byte x,
          byte y,
          ushort plant,
          ushort index,
          string newText)
        {
            BarricadeRegion region;
            if (!this.channel.checkServer(steamID) || !BarricadeManager.tryGetRegion(x, y, plant, out region) || !Provider.isServer && !region.isNetworked || (int)index >= region.drops.Count)
                return;
            InteractableSign interactable = region.drops[(int)index].interactable as InteractableSign;
            if (!((UnityEngine.Object)interactable != (UnityEngine.Object)null))
                return;
            interactable.updateText(newText);
        }

        [SteamCall(ESteamCallValidation.SERVERSIDE)]
        public void askUpdateSign(
          CSteamID steamID,
          byte x,
          byte y,
          ushort plant,
          ushort index,
          string newText)
        {
            BarricadeRegion region;
            if (!Provider.isServer || !BarricadeManager.tryGetRegion(x, y, plant, out region))
                return;
            Player player = PlayerTool.getPlayer(steamID);
            if ((UnityEngine.Object)player == (UnityEngine.Object)null || player.life.isDead || ((int)index >= region.drops.Count || !player.tryToPerformRateLimitedAction()) || (Encoding.UTF8.GetByteCount(newText) > (int)byte.MaxValue || newText.Contains("<size")) || (double)(region.drops[(int)index].model.transform.position - player.transform.position).sqrMagnitude > 400.0)
                return;
            InteractableSign interactable = region.drops[(int)index].interactable as InteractableSign;
            if (!((UnityEngine.Object)interactable != (UnityEngine.Object)null) || !interactable.checkUpdate(player.channel.owner.playerID.steamID, player.quests.groupID))
                return;
            bool shouldAllow = true;
            if (BarricadeManager.onModifySignRequested != null)
                BarricadeManager.onModifySignRequested(steamID, interactable, ref newText, ref shouldAllow);
            if (!shouldAllow)
                return;
            if (plant == ushort.MaxValue)
                BarricadeManager.manager.channel.send("tellUpdateSign", ESteamCall.ALL, x, y, BarricadeManager.BARRICADE_REGIONS, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)newText);
            else
                BarricadeManager.manager.channel.send("tellUpdateSign", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)newText);
            byte[] state = region.barricades[(int)index].barricade.state;
            byte[] bytes = Encoding.UTF8.GetBytes(newText);
            byte[] numArray = new byte[17 + bytes.Length];
            Buffer.BlockCopy((Array)state, 0, (Array)numArray, 0, 16);
            numArray[16] = (byte)bytes.Length;
            if (bytes.Length > 0)
                Buffer.BlockCopy((Array)bytes, 0, (Array)numArray, 17, bytes.Length);
            region.barricades[(int)index].barricade.state = numArray;
        }

        public static void updateStereoTrack(Transform barricade, Guid newTrack)
        {
            byte x;
            byte y;
            ushort plant;
            ushort index;
            BarricadeRegion region;
            if (!BarricadeManager.tryGetInfo(barricade, out x, out y, out plant, out index, out region))
                return;
            BarricadeManager.manager.channel.send("askUpdateStereoTrack", ESteamCall.SERVER, ESteamPacket.UPDATE_UNRELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)newTrack);
        }

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public void tellUpdateStereoTrack(
          CSteamID steamID,
          byte x,
          byte y,
          ushort plant,
          ushort index,
          Guid newTrack)
        {
            BarricadeRegion region;
            if (!this.channel.checkServer(steamID) || !BarricadeManager.tryGetRegion(x, y, plant, out region) || !Provider.isServer && !region.isNetworked || (int)index >= region.drops.Count)
                return;
            InteractableStereo interactable = region.drops[(int)index].interactable as InteractableStereo;
            if (!((UnityEngine.Object)interactable != (UnityEngine.Object)null))
                return;
            interactable.updateTrack(newTrack);
        }

        //[SteamCall(ESteamCallValidation.SERVERSIDE)]
        //public void askUpdateStereoTrack(
        //  CSteamID steamID,
        //  byte x,
        //  byte y,
        //  ushort plant,
        //  ushort index,
        //  Guid newTrack)
        //{
        //    BarricadeRegion region;
        //    if (!Provider.isServer || !BarricadeManager.tryGetRegion(x, y, plant, out region))
        //        return;
        //    Player player = PlayerTool.getPlayer(steamID);
        //    if ((UnityEngine.Object)player == (UnityEngine.Object)null || player.life.isDead || ((int)index >= region.drops.Count || !player.tryToPerformRateLimitedAction()) || ((double)(region.drops[(int)index].model.transform.position - player.transform.position).sqrMagnitude > 400.0 || !((UnityEngine.Object)(region.drops[(int)index].interactable as InteractableStereo) != (UnityEngine.Object)null)))
        //        return;
        //    if (plant == ushort.MaxValue)
        //        BarricadeManager.manager.channel.send("tellUpdateStereoTrack", ESteamCall.ALL, x, y, BarricadeManager.BARRICADE_REGIONS, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)newTrack);
        //    else
        //        BarricadeManager.manager.channel.send("tellUpdateStereoTrack", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)newTrack);
        //    byte[] state = region.barricades[(int)index].barricade.state;
        //    new GuidBuffer(newTrack).Write(state, 0);
        //}

        public static void updateStereoVolume(Transform barricade, byte newVolume)
        {
            byte x;
            byte y;
            ushort plant;
            ushort index;
            BarricadeRegion region;
            if (!BarricadeManager.tryGetInfo(barricade, out x, out y, out plant, out index, out region))
                return;
            BarricadeManager.manager.channel.send("askUpdateStereoVolume", ESteamCall.SERVER, ESteamPacket.UPDATE_UNRELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)newVolume);
        }

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public void tellUpdateStereoVolume(
          CSteamID steamID,
          byte x,
          byte y,
          ushort plant,
          ushort index,
          byte newVolume)
        {
            BarricadeRegion region;
            if (!this.channel.checkServer(steamID) || !BarricadeManager.tryGetRegion(x, y, plant, out region) || !Provider.isServer && !region.isNetworked || (int)index >= region.drops.Count)
                return;
            InteractableStereo interactable = region.drops[(int)index].interactable as InteractableStereo;
            if (!((UnityEngine.Object)interactable != (UnityEngine.Object)null))
                return;
            interactable.compressedVolume = newVolume;
        }

        [SteamCall(ESteamCallValidation.SERVERSIDE)]
        public void askUpdateStereoVolume(
          CSteamID steamID,
          byte x,
          byte y,
          ushort plant,
          ushort index,
          byte newVolume)
        {
            BarricadeRegion region;
            if (!Provider.isServer || !BarricadeManager.tryGetRegion(x, y, plant, out region))
                return;
            Player player = PlayerTool.getPlayer(steamID);
            if ((UnityEngine.Object)player == (UnityEngine.Object)null || player.life.isDead || ((int)index >= region.drops.Count || !player.tryToPerformRateLimitedAction()) || ((double)(region.drops[(int)index].model.transform.position - player.transform.position).sqrMagnitude > 400.0 || !((UnityEngine.Object)(region.drops[(int)index].interactable as InteractableStereo) != (UnityEngine.Object)null)))
                return;
            if (plant == ushort.MaxValue)
                BarricadeManager.manager.channel.send("tellUpdateStereoVolume", ESteamCall.ALL, x, y, BarricadeManager.BARRICADE_REGIONS, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)newVolume);
            else
                BarricadeManager.manager.channel.send("tellUpdateStereoVolume", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)newVolume);
            region.barricades[(int)index].barricade.state[16] = newVolume;
        }

        public static void transferLibrary(Transform barricade, byte transaction, uint delta)
        {
            byte x;
            byte y;
            ushort plant;
            ushort index;
            BarricadeRegion region;
            if (!BarricadeManager.tryGetInfo(barricade, out x, out y, out plant, out index, out region))
                return;
            BarricadeManager.manager.channel.send("askTransferLibrary", ESteamCall.SERVER, ESteamPacket.UPDATE_UNRELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)transaction, (object)delta);
        }

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public void tellTransferLibrary(
          CSteamID steamID,
          byte x,
          byte y,
          ushort plant,
          ushort index,
          uint newAmount)
        {
            BarricadeRegion region;
            if (!this.channel.checkServer(steamID) || !BarricadeManager.tryGetRegion(x, y, plant, out region) || !Provider.isServer && !region.isNetworked || (int)index >= region.drops.Count)
                return;
            InteractableLibrary interactable = region.drops[(int)index].interactable as InteractableLibrary;
            if (!((UnityEngine.Object)interactable != (UnityEngine.Object)null))
                return;
            interactable.updateAmount(newAmount);
        }

        [SteamCall(ESteamCallValidation.SERVERSIDE)]
        public void askTransferLibrary(
          CSteamID steamID,
          byte x,
          byte y,
          ushort plant,
          ushort index,
          byte transaction,
          uint delta)
        {
            BarricadeRegion region;
            if (!Provider.isServer || !BarricadeManager.tryGetRegion(x, y, plant, out region))
                return;
            Player player = PlayerTool.getPlayer(steamID);
            if ((UnityEngine.Object)player == (UnityEngine.Object)null || player.life.isDead || ((int)index >= region.drops.Count || !player.tryToPerformRateLimitedAction()) || (double)(region.drops[(int)index].model.transform.position - player.transform.position).sqrMagnitude > 400.0)
                return;
            InteractableLibrary interactable = region.drops[(int)index].interactable as InteractableLibrary;
            if (!((UnityEngine.Object)interactable != (UnityEngine.Object)null) || !interactable.checkTransfer(player.channel.owner.playerID.steamID, player.quests.groupID))
                return;
            uint num1;
            if (transaction == (byte)0)
            {
                uint num2 = (uint)Math.Ceiling((double)delta * ((double)interactable.tax / 100.0));
                uint num3 = delta - num2;
                if (delta > player.skills.experience || num3 + interactable.amount > interactable.capacity)
                    return;
                num1 = interactable.amount + num3;
                player.skills.askSpend(delta);
            }
            else
            {
                if (delta > interactable.amount)
                    return;
                num1 = interactable.amount - delta;
                player.skills.askAward(delta);
            }
            if (plant == ushort.MaxValue)
                BarricadeManager.manager.channel.send("tellTransferLibrary", ESteamCall.ALL, x, y, BarricadeManager.BARRICADE_REGIONS, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)num1);
            else
                BarricadeManager.manager.channel.send("tellTransferLibrary", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)num1);
            Buffer.BlockCopy((Array)BitConverter.GetBytes(num1), 0, (Array)region.barricades[(int)index].barricade.state, 16, 4);
        }

        public static void toggleSafezone(Transform barricade)
        {
            byte x;
            byte y;
            ushort plant;
            ushort index;
            BarricadeRegion region;
            if (!BarricadeManager.tryGetInfo(barricade, out x, out y, out plant, out index, out region))
                return;
            BarricadeManager.manager.channel.send("askToggleSafezone", ESteamCall.SERVER, ESteamPacket.UPDATE_UNRELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index);
        }

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public void tellToggleSafezone(
          CSteamID steamID,
          byte x,
          byte y,
          ushort plant,
          ushort index,
          bool isPowered)
        {
            BarricadeRegion region;
            if (!this.channel.checkServer(steamID) || !BarricadeManager.tryGetRegion(x, y, plant, out region) || !Provider.isServer && !region.isNetworked || (int)index >= region.drops.Count)
                return;
            InteractableSafezone interactable = region.drops[(int)index].interactable as InteractableSafezone;
            if (!((UnityEngine.Object)interactable != (UnityEngine.Object)null))
                return;
            interactable.updatePowered(isPowered);
        }

        [SteamCall(ESteamCallValidation.SERVERSIDE)]
        public void askToggleSafezone(CSteamID steamID, byte x, byte y, ushort plant, ushort index)
        {
            BarricadeRegion region;
            if (!Provider.isServer || !BarricadeManager.tryGetRegion(x, y, plant, out region))
                return;
            Player player = PlayerTool.getPlayer(steamID);
            if ((UnityEngine.Object)player == (UnityEngine.Object)null || player.life.isDead || ((int)index >= region.drops.Count || !player.tryToPerformRateLimitedAction()) || (double)(region.drops[(int)index].model.transform.position - player.transform.position).sqrMagnitude > 400.0)
                return;
            InteractableSafezone interactable = region.drops[(int)index].interactable as InteractableSafezone;
            if (!((UnityEngine.Object)interactable != (UnityEngine.Object)null))
                return;
            if (plant == ushort.MaxValue)
                BarricadeManager.manager.channel.send("tellToggleSafezone", ESteamCall.ALL, x, y, BarricadeManager.BARRICADE_REGIONS, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)!interactable.isPowered);
            else
                BarricadeManager.manager.channel.send("tellToggleSafezone", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)!interactable.isPowered);
            region.barricades[(int)index].barricade.state[0] = !interactable.isPowered ? (byte)0 : (byte)1;
            EffectManager.sendEffect((ushort)8, EffectManager.SMALL, interactable.transform.position);
        }

        public static void toggleOxygenator(Transform barricade)
        {
            byte x;
            byte y;
            ushort plant;
            ushort index;
            BarricadeRegion region;
            if (!BarricadeManager.tryGetInfo(barricade, out x, out y, out plant, out index, out region))
                return;
            BarricadeManager.manager.channel.send("askToggleOxygenator", ESteamCall.SERVER, ESteamPacket.UPDATE_UNRELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index);
        }

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public void tellToggleOxygenator(
          CSteamID steamID,
          byte x,
          byte y,
          ushort plant,
          ushort index,
          bool isPowered)
        {
            BarricadeRegion region;
            if (!this.channel.checkServer(steamID) || !BarricadeManager.tryGetRegion(x, y, plant, out region) || !Provider.isServer && !region.isNetworked || (int)index >= region.drops.Count)
                return;
            InteractableOxygenator interactable = region.drops[(int)index].interactable as InteractableOxygenator;
            if (!((UnityEngine.Object)interactable != (UnityEngine.Object)null))
                return;
            interactable.updatePowered(isPowered);
        }

        [SteamCall(ESteamCallValidation.SERVERSIDE)]
        public void askToggleOxygenator(CSteamID steamID, byte x, byte y, ushort plant, ushort index)
        {
            BarricadeRegion region;
            if (!Provider.isServer || !BarricadeManager.tryGetRegion(x, y, plant, out region))
                return;
            Player player = PlayerTool.getPlayer(steamID);
            if ((UnityEngine.Object)player == (UnityEngine.Object)null || player.life.isDead || ((int)index >= region.drops.Count || !player.tryToPerformRateLimitedAction()) || (double)(region.drops[(int)index].model.transform.position - player.transform.position).sqrMagnitude > 400.0)
                return;
            InteractableOxygenator interactable = region.drops[(int)index].interactable as InteractableOxygenator;
            if (!((UnityEngine.Object)interactable != (UnityEngine.Object)null))
                return;
            if (plant == ushort.MaxValue)
                BarricadeManager.manager.channel.send("tellToggleOxygenator", ESteamCall.ALL, x, y, BarricadeManager.BARRICADE_REGIONS, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)!interactable.isPowered);
            else
                BarricadeManager.manager.channel.send("tellToggleOxygenator", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)!interactable.isPowered);
            region.barricades[(int)index].barricade.state[0] = !interactable.isPowered ? (byte)0 : (byte)1;
            EffectManager.sendEffect((ushort)8, EffectManager.SMALL, interactable.transform.position);
        }

        public static void toggleSpot(Transform barricade)
        {
            byte x;
            byte y;
            ushort plant;
            ushort index;
            BarricadeRegion region;
            if (!BarricadeManager.tryGetInfo(barricade, out x, out y, out plant, out index, out region))
                return;
            BarricadeManager.manager.channel.send("askToggleSpot", ESteamCall.SERVER, ESteamPacket.UPDATE_UNRELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index);
        }

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public void tellToggleSpot(
          CSteamID steamID,
          byte x,
          byte y,
          ushort plant,
          ushort index,
          bool isPowered)
        {
            BarricadeRegion region;
            if (!this.channel.checkServer(steamID) || !BarricadeManager.tryGetRegion(x, y, plant, out region) || !Provider.isServer && !region.isNetworked || (int)index >= region.drops.Count)
                return;
            InteractableSpot interactable = region.drops[(int)index].interactable as InteractableSpot;
            if (!((UnityEngine.Object)interactable != (UnityEngine.Object)null))
                return;
            interactable.updatePowered(isPowered);
        }

        [SteamCall(ESteamCallValidation.SERVERSIDE)]
        public void askToggleSpot(CSteamID steamID, byte x, byte y, ushort plant, ushort index)
        {
            BarricadeRegion region;
            if (!Provider.isServer || !BarricadeManager.tryGetRegion(x, y, plant, out region))
                return;
            Player player = PlayerTool.getPlayer(steamID);
            if ((UnityEngine.Object)player == (UnityEngine.Object)null || player.life.isDead || ((int)index >= region.drops.Count || !player.tryToPerformRateLimitedAction()) || (double)(region.drops[(int)index].model.transform.position - player.transform.position).sqrMagnitude > 400.0)
                return;
            InteractableSpot interactable = region.drops[(int)index].interactable as InteractableSpot;
            if (!((UnityEngine.Object)interactable != (UnityEngine.Object)null))
                return;
            if (plant == ushort.MaxValue)
                BarricadeManager.manager.channel.send("tellToggleSpot", ESteamCall.ALL, x, y, BarricadeManager.BARRICADE_REGIONS, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)!interactable.isPowered);
            else
                BarricadeManager.manager.channel.send("tellToggleSpot", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)!interactable.isPowered);
            region.barricades[(int)index].barricade.state[0] = !interactable.isPowered ? (byte)0 : (byte)1;
            EffectManager.sendEffect((ushort)8, EffectManager.SMALL, interactable.transform.position);
        }

        public static void sendFuel(Transform barricade, ushort fuel)
        {
            byte x;
            byte y;
            ushort plant;
            ushort index;
            BarricadeRegion region;
            if (!BarricadeManager.tryGetInfo(barricade, out x, out y, out plant, out index, out region))
                return;
            if (plant == ushort.MaxValue)
                BarricadeManager.manager.channel.send("tellFuel", ESteamCall.ALL, x, y, BarricadeManager.BARRICADE_REGIONS, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)fuel);
            else
                BarricadeManager.manager.channel.send("tellFuel", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)fuel);
        }

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public void tellFuel(
          CSteamID steamID,
          byte x,
          byte y,
          ushort plant,
          ushort index,
          ushort fuel)
        {
            BarricadeRegion region;
            if (!this.channel.checkServer(steamID) || !BarricadeManager.tryGetRegion(x, y, plant, out region) || !Provider.isServer && !region.isNetworked || (int)index >= region.drops.Count)
                return;
            InteractableGenerator interactable = region.drops[(int)index].interactable as InteractableGenerator;
            if (!((UnityEngine.Object)interactable != (UnityEngine.Object)null))
                return;
            interactable.tellFuel(fuel);
        }

        public static void toggleGenerator(Transform barricade)
        {
            byte x;
            byte y;
            ushort plant;
            ushort index;
            BarricadeRegion region;
            if (!BarricadeManager.tryGetInfo(barricade, out x, out y, out plant, out index, out region))
                return;
            BarricadeManager.manager.channel.send("askToggleGenerator", ESteamCall.SERVER, ESteamPacket.UPDATE_UNRELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index);
        }

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public void tellToggleGenerator(
          CSteamID steamID,
          byte x,
          byte y,
          ushort plant,
          ushort index,
          bool isPowered)
        {
            BarricadeRegion region;
            if (!this.channel.checkServer(steamID) || !BarricadeManager.tryGetRegion(x, y, plant, out region) || !Provider.isServer && !region.isNetworked || (int)index >= region.drops.Count)
                return;
            InteractableGenerator interactable = region.drops[(int)index].interactable as InteractableGenerator;
            if (!((UnityEngine.Object)interactable != (UnityEngine.Object)null))
                return;
            interactable.updatePowered(isPowered);
        }

        [SteamCall(ESteamCallValidation.SERVERSIDE)]
        public void askToggleGenerator(CSteamID steamID, byte x, byte y, ushort plant, ushort index)
        {
            BarricadeRegion region;
            if (!Provider.isServer || !BarricadeManager.tryGetRegion(x, y, plant, out region))
                return;
            Player player = PlayerTool.getPlayer(steamID);
            if ((UnityEngine.Object)player == (UnityEngine.Object)null || player.life.isDead || ((int)index >= region.drops.Count || !player.tryToPerformRateLimitedAction()) || (double)(region.drops[(int)index].model.transform.position - player.transform.position).sqrMagnitude > 400.0)
                return;
            InteractableGenerator interactable = region.drops[(int)index].interactable as InteractableGenerator;
            if (!((UnityEngine.Object)interactable != (UnityEngine.Object)null))
                return;
            if (plant == ushort.MaxValue)
                BarricadeManager.manager.channel.send("tellToggleGenerator", ESteamCall.ALL, x, y, BarricadeManager.BARRICADE_REGIONS, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)!interactable.isPowered);
            else
                BarricadeManager.manager.channel.send("tellToggleGenerator", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)!interactable.isPowered);
            region.barricades[(int)index].barricade.state[0] = !interactable.isPowered ? (byte)0 : (byte)1;
            EffectManager.sendEffect((ushort)8, EffectManager.SMALL, interactable.transform.position);
        }

        public static void toggleFire(Transform barricade)
        {
            byte x;
            byte y;
            ushort plant;
            ushort index;
            BarricadeRegion region;
            if (!BarricadeManager.tryGetInfo(barricade, out x, out y, out plant, out index, out region))
                return;
            BarricadeManager.manager.channel.send("askToggleFire", ESteamCall.SERVER, ESteamPacket.UPDATE_UNRELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index);
        }

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public void tellToggleFire(
          CSteamID steamID,
          byte x,
          byte y,
          ushort plant,
          ushort index,
          bool isLit)
        {
            BarricadeRegion region;
            if (!this.channel.checkServer(steamID) || !BarricadeManager.tryGetRegion(x, y, plant, out region) || !Provider.isServer && !region.isNetworked || (int)index >= region.drops.Count)
                return;
            InteractableFire interactable = region.drops[(int)index].interactable as InteractableFire;
            if (!((UnityEngine.Object)interactable != (UnityEngine.Object)null))
                return;
            interactable.updateLit(isLit);
        }

        [SteamCall(ESteamCallValidation.SERVERSIDE)]
        public void askToggleFire(CSteamID steamID, byte x, byte y, ushort plant, ushort index)
        {
            BarricadeRegion region;
            if (!Provider.isServer || !BarricadeManager.tryGetRegion(x, y, plant, out region))
                return;
            Player player = PlayerTool.getPlayer(steamID);
            if ((UnityEngine.Object)player == (UnityEngine.Object)null || player.life.isDead || ((int)index >= region.drops.Count || !player.tryToPerformRateLimitedAction()) || (double)(region.drops[(int)index].model.transform.position - player.transform.position).sqrMagnitude > 400.0)
                return;
            InteractableFire interactable = region.drops[(int)index].interactable as InteractableFire;
            if (!((UnityEngine.Object)interactable != (UnityEngine.Object)null))
                return;
            if (plant == ushort.MaxValue)
                BarricadeManager.manager.channel.send("tellToggleFire", ESteamCall.ALL, x, y, BarricadeManager.BARRICADE_REGIONS, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)!interactable.isLit);
            else
                BarricadeManager.manager.channel.send("tellToggleFire", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)!interactable.isLit);
            region.barricades[(int)index].barricade.state[0] = !interactable.isLit ? (byte)0 : (byte)1;
        }

        public static void toggleOven(Transform barricade)
        {
            byte x;
            byte y;
            ushort plant;
            ushort index;
            BarricadeRegion region;
            if (!BarricadeManager.tryGetInfo(barricade, out x, out y, out plant, out index, out region))
                return;
            BarricadeManager.manager.channel.send("askToggleOven", ESteamCall.SERVER, ESteamPacket.UPDATE_UNRELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index);
        }

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public void tellToggleOven(
          CSteamID steamID,
          byte x,
          byte y,
          ushort plant,
          ushort index,
          bool isLit)
        {
            BarricadeRegion region;
            if (!this.channel.checkServer(steamID) || !BarricadeManager.tryGetRegion(x, y, plant, out region) || !Provider.isServer && !region.isNetworked || (int)index >= region.drops.Count)
                return;
            InteractableOven interactable = region.drops[(int)index].interactable as InteractableOven;
            if (!((UnityEngine.Object)interactable != (UnityEngine.Object)null))
                return;
            interactable.updateLit(isLit);
        }

        [SteamCall(ESteamCallValidation.SERVERSIDE)]
        public void askToggleOven(CSteamID steamID, byte x, byte y, ushort plant, ushort index)
        {
            BarricadeRegion region;
            if (!Provider.isServer || !BarricadeManager.tryGetRegion(x, y, plant, out region))
                return;
            Player player = PlayerTool.getPlayer(steamID);
            if ((UnityEngine.Object)player == (UnityEngine.Object)null || player.life.isDead || ((int)index >= region.drops.Count || !player.tryToPerformRateLimitedAction()) || (double)(region.drops[(int)index].model.transform.position - player.transform.position).sqrMagnitude > 400.0)
                return;
            InteractableOven interactable = region.drops[(int)index].interactable as InteractableOven;
            if (!((UnityEngine.Object)interactable != (UnityEngine.Object)null))
                return;
            if (plant == ushort.MaxValue)
                BarricadeManager.manager.channel.send("tellToggleOven", ESteamCall.ALL, x, y, BarricadeManager.BARRICADE_REGIONS, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)!interactable.isLit);
            else
                BarricadeManager.manager.channel.send("tellToggleOven", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)!interactable.isLit);
            region.barricades[(int)index].barricade.state[0] = !interactable.isLit ? (byte)0 : (byte)1;
        }

        public static void farm(Transform barricade)
        {
            byte x;
            byte y;
            ushort plant;
            ushort index;
            BarricadeRegion region;
            if (!BarricadeManager.tryGetInfo(barricade, out x, out y, out plant, out index, out region))
                return;
            BarricadeManager.manager.channel.send("askFarm", ESteamCall.SERVER, ESteamPacket.UPDATE_UNRELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index);
        }

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public void tellFarm(
          CSteamID steamID,
          byte x,
          byte y,
          ushort plant,
          ushort index,
          uint planted)
        {
            BarricadeRegion region;
            if (!this.channel.checkServer(steamID) || !BarricadeManager.tryGetRegion(x, y, plant, out region) || !Provider.isServer && !region.isNetworked || (int)index >= region.drops.Count)
                return;
            InteractableFarm interactable = region.drops[(int)index].interactable as InteractableFarm;
            if (!((UnityEngine.Object)interactable != (UnityEngine.Object)null))
                return;
            interactable.updatePlanted(planted);
        }

        //[SteamCall(ESteamCallValidation.SERVERSIDE)]
        //public void askFarm(CSteamID steamID, byte x, byte y, ushort plant, ushort index)
        //{
        //    BarricadeRegion region;
        //    if (!Provider.isServer || !BarricadeManager.tryGetRegion(x, y, plant, out region))
        //        return;
        //    Player player = PlayerTool.getPlayer(steamID);
        //    if ((UnityEngine.Object)player == (UnityEngine.Object)null || player.life.isDead || ((int)index >= region.drops.Count || !player.tryToPerformRateLimitedAction()) || (double)(region.drops[(int)index].model.transform.position - player.transform.position).sqrMagnitude > 400.0)
        //        return;
        //    bool shouldAllow = true;
        //    if (BarricadeManager.onHarvestPlantRequested != null)
        //        BarricadeManager.onHarvestPlantRequested(steamID, x, y, plant, index, ref shouldAllow);
        //    if (!shouldAllow)
        //        return;
        //    InteractableFarm interactable = region.drops[(int)index].interactable as InteractableFarm;
        //    if (!((UnityEngine.Object)interactable != (UnityEngine.Object)null) || !interactable.checkFarm())
        //        return;
        //    player.inventory.forceAddItem(new Item(interactable.grow, EItemOrigin.NATURE), true);
        //    if ((double)UnityEngine.Random.value < (double)player.skills.mastery(2, 5))
        //        player.inventory.forceAddItem(new Item(interactable.grow, EItemOrigin.NATURE), true);
        //    BarricadeManager.damage(interactable.transform, 2f, 1f, false, new CSteamID(), EDamageOrigin.Plant_Harvested);
        //    player.sendStat(EPlayerStat.FOUND_PLANTS);
        //    player.skills.askPay(1U);
        //}

        public static void updateFarm(Transform barricade, uint planted, bool shouldSend)
        {
            byte x;
            byte y;
            ushort plant;
            ushort index;
            BarricadeRegion region;
            if (!BarricadeManager.tryGetInfo(barricade, out x, out y, out plant, out index, out region))
                return;
            if (shouldSend)
            {
                if (plant == ushort.MaxValue)
                    BarricadeManager.manager.channel.send("tellFarm", ESteamCall.ALL, x, y, BarricadeManager.BARRICADE_REGIONS, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)planted);
                else
                    BarricadeManager.manager.channel.send("tellFarm", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)planted);
            }
            BitConverter.GetBytes(planted).CopyTo((Array)region.barricades[(int)index].barricade.state, 0);
        }

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public void tellOil(
          CSteamID steamID,
          byte x,
          byte y,
          ushort plant,
          ushort index,
          ushort fuel)
        {
            BarricadeRegion region;
            if (!this.channel.checkServer(steamID) || !BarricadeManager.tryGetRegion(x, y, plant, out region) || !Provider.isServer && !region.isNetworked || (int)index >= region.drops.Count)
                return;
            InteractableOil interactable = region.drops[(int)index].interactable as InteractableOil;
            if (!((UnityEngine.Object)interactable != (UnityEngine.Object)null))
                return;
            interactable.tellFuel(fuel);
        }

        public static void sendOil(Transform barricade, ushort fuel)
        {
            byte x;
            byte y;
            ushort plant;
            ushort index;
            BarricadeRegion region;
            if (!BarricadeManager.tryGetInfo(barricade, out x, out y, out plant, out index, out region))
                return;
            if (plant == ushort.MaxValue)
                BarricadeManager.manager.channel.send("tellOil", ESteamCall.ALL, x, y, BarricadeManager.BARRICADE_REGIONS, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)fuel);
            else
                BarricadeManager.manager.channel.send("tellOil", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)fuel);
        }

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public void tellRainBarrel(
          CSteamID steamID,
          byte x,
          byte y,
          ushort plant,
          ushort index,
          bool isFull)
        {
            BarricadeRegion region;
            if (!this.channel.checkServer(steamID) || !BarricadeManager.tryGetRegion(x, y, plant, out region) || !Provider.isServer && !region.isNetworked || (int)index >= region.drops.Count)
                return;
            InteractableRainBarrel interactable = region.drops[(int)index].interactable as InteractableRainBarrel;
            if (!((UnityEngine.Object)interactable != (UnityEngine.Object)null))
                return;
            interactable.updateFull(isFull);
        }

        public static void updateRainBarrel(Transform barricade, bool isFull, bool shouldSend)
        {
            byte x;
            byte y;
            ushort plant;
            ushort index;
            BarricadeRegion region;
            if (!BarricadeManager.tryGetInfo(barricade, out x, out y, out plant, out index, out region))
                return;
            if (shouldSend)
            {
                if (plant == ushort.MaxValue)
                    BarricadeManager.manager.channel.send("tellRainBarrel", ESteamCall.ALL, x, y, BarricadeManager.BARRICADE_REGIONS, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)isFull);
                else
                    BarricadeManager.manager.channel.send("tellRainBarrel", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)isFull);
            }
            region.barricades[(int)index].barricade.state[0] = !isFull ? (byte)0 : (byte)1;
        }

        public static void sendStorageDisplay(
          Transform barricade,
          Item item,
          ushort skin,
          ushort mythic,
          string tags,
          string dynamicProps)
        {
            byte x;
            byte y;
            ushort plant;
            ushort index;
            BarricadeRegion region;
            if (!BarricadeManager.tryGetInfo(barricade, out x, out y, out plant, out index, out region))
                return;
            if (item != null)
                BarricadeManager.manager.channel.send("tellStorageDisplay", ESteamCall.CLIENTS, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)item.id, (object)item.quality, (object)item.state, (object)skin, (object)mythic, (object)tags, (object)dynamicProps);
            else
                BarricadeManager.manager.channel.send("tellStorageDisplay", ESteamCall.CLIENTS, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)(ushort)0, (object)(byte)0, (object)(byte)0, (object)skin, (object)mythic, (object)tags, (object)dynamicProps);
        }

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public void tellStorageDisplay(
          CSteamID steamID,
          byte x,
          byte y,
          ushort plant,
          ushort index,
          ushort id,
          byte quality,
          byte[] state,
          ushort skin,
          ushort mythic,
          string tags,
          string dynamicProps)
        {
            BarricadeRegion region;
            if (!this.channel.checkServer(steamID) || !BarricadeManager.tryGetRegion(x, y, plant, out region) || !Provider.isServer && !region.isNetworked || (int)index >= region.drops.Count)
                return;
            InteractableStorage interactable = region.drops[(int)index].interactable as InteractableStorage;
            if (!((UnityEngine.Object)interactable != (UnityEngine.Object)null))
                return;
            interactable.setDisplay(id, quality, state, skin, mythic, tags, dynamicProps);
        }

        public static void storeStorage(Transform barricade, bool quickGrab)
        {
            byte x;
            byte y;
            ushort plant;
            ushort index;
            BarricadeRegion region;
            if (!BarricadeManager.tryGetInfo(barricade, out x, out y, out plant, out index, out region))
                return;
            BarricadeManager.manager.channel.send("askStoreStorage", ESteamCall.SERVER, ESteamPacket.UPDATE_UNRELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)quickGrab);
        }

        [SteamCall(ESteamCallValidation.SERVERSIDE)]
        public void askStoreStorage(
          CSteamID steamID,
          byte x,
          byte y,
          ushort plant,
          ushort index,
          bool quickGrab)
        {
            BarricadeRegion region;
            if (!Provider.isServer || !BarricadeManager.tryGetRegion(x, y, plant, out region))
                return;
            Player player = PlayerTool.getPlayer(steamID);
            if ((UnityEngine.Object)player == (UnityEngine.Object)null || player.life.isDead || (player.inventory.isStoring || player.animator.gesture == EPlayerGesture.ARREST_START) || ((int)index >= region.drops.Count || !player.tryToPerformRateLimitedAction()))
                return;
            Vector3 position = region.drops[(int)index].model.transform.position;
            if ((double)(position - player.transform.position).sqrMagnitude > 400.0 || Physics.Linecast(player.look.getEyesPosition(), position, RayMasks.BLOCK_BARRICADE_INTERACT_LOS, (QueryTriggerInteraction)1))
                return;
            InteractableStorage interactable = region.drops[(int)index].interactable as InteractableStorage;
            if (!((UnityEngine.Object)interactable != (UnityEngine.Object)null))
                return;
            if (interactable.checkStore(player.channel.owner.playerID.steamID, player.quests.groupID))
            {
                bool shouldAllow = true;
                if (BarricadeManager.onOpenStorageRequested != null)
                    BarricadeManager.onOpenStorageRequested(steamID, interactable, ref shouldAllow);
                if (!shouldAllow)
                    return;
                if (interactable.isDisplay && quickGrab)
                {
                    if (interactable.displayItem == null)
                        return;
                    player.inventory.forceAddItem(interactable.displayItem, true);
                    interactable.displayItem = (Item)null;
                    interactable.displaySkin = (ushort)0;
                    interactable.displayMythic = (ushort)0;
                    interactable.displayTags = string.Empty;
                    interactable.displayDynamicProps = string.Empty;
                    interactable.items.removeItem((byte)0);
                }
                else
                {
                    interactable.isOpen = true;
                    interactable.opener = player;
                    player.inventory.isStoring = true;
                    player.inventory.isStorageTrunk = false;
                    player.inventory.storage = interactable;
                    player.inventory.updateItems(PlayerInventory.STORAGE, interactable.items);
                    player.inventory.sendStorage();
                }
            }
            else
                player.sendMessage(EPlayerMessage.BUSY);
        }

        public static void toggleDoor(Transform barricade)
        {
            byte x;
            byte y;
            ushort plant;
            ushort index;
            BarricadeRegion region;
            if (!BarricadeManager.tryGetInfo(barricade, out x, out y, out plant, out index, out region))
                return;
            BarricadeManager.manager.channel.send("askToggleDoor", ESteamCall.SERVER, ESteamPacket.UPDATE_UNRELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index);
        }

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public void tellToggleDoor(
          CSteamID steamID,
          byte x,
          byte y,
          ushort plant,
          ushort index,
          bool isOpen)
        {
            BarricadeRegion region;
            if (!this.channel.checkServer(steamID) || !BarricadeManager.tryGetRegion(x, y, plant, out region) || !Provider.isServer && !region.isNetworked || (int)index >= region.drops.Count)
                return;
            InteractableDoor interactable = region.drops[(int)index].interactable as InteractableDoor;
            if (!((UnityEngine.Object)interactable != (UnityEngine.Object)null))
                return;
            interactable.updateToggle(isOpen);
        }

        [SteamCall(ESteamCallValidation.SERVERSIDE)]
        public void askToggleDoor(CSteamID steamID, byte x, byte y, ushort plant, ushort index)
        {
            BarricadeRegion region;
            if (!Provider.isServer || !BarricadeManager.tryGetRegion(x, y, plant, out region))
                return;
            Player player = PlayerTool.getPlayer(steamID);
            if ((UnityEngine.Object)player == (UnityEngine.Object)null || player.life.isDead || ((int)index >= region.drops.Count || !player.tryToPerformRateLimitedAction()) || (double)(region.drops[(int)index].model.transform.position - player.transform.position).sqrMagnitude > 400.0)
                return;
            InteractableDoor interactable = region.drops[(int)index].interactable as InteractableDoor;
            if (!((UnityEngine.Object)interactable != (UnityEngine.Object)null) || !interactable.isOpenable || !interactable.checkToggle(player.channel.owner.playerID.steamID, player.quests.groupID))
                return;
            if (plant == ushort.MaxValue)
                BarricadeManager.manager.channel.send("tellToggleDoor", ESteamCall.ALL, x, y, BarricadeManager.BARRICADE_REGIONS, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)!interactable.isOpen);
            else
                BarricadeManager.manager.channel.send("tellToggleDoor", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)!interactable.isOpen);
            region.barricades[(int)index].barricade.state[16] = !interactable.isOpen ? (byte)0 : (byte)1;
        }

        public static bool tryGetBed(CSteamID owner, out Vector3 point, out byte angle)
        {
            point = Vector3.zero;
            angle = (byte)0;
            for (byte index1 = 0; (int)index1 < (int)Regions.WORLD_SIZE; ++index1)
            {
                for (byte index2 = 0; (int)index2 < (int)Regions.WORLD_SIZE; ++index2)
                {
                    BarricadeRegion region = BarricadeManager.regions[(int)index1, (int)index2];
                    for (ushort index3 = 0; (int)index3 < BarricadeManager.regions[(int)index1, (int)index2].barricades.Count; ++index3)
                    {
                        if (region.barricades[(int)index3].barricade.state.Length > 0)
                        {
                            if ((int)index3 < region.drops.Count)
                            {
                                InteractableBed interactable = region.drops[(int)index3].interactable as InteractableBed;
                                if ((UnityEngine.Object)interactable != (UnityEngine.Object)null && interactable.owner == owner && Level.checkSafeIncludingClipVolumes(interactable.transform.position))
                                {
                                    point = interactable.transform.position;
                                    angle = MeasurementTool.angleToByte((float)((int)region.barricades[(int)index3].angle_y * 2 + 90));
                                    int num = Physics.OverlapCapsuleNonAlloc(point + new Vector3(0.0f, PlayerStance.RADIUS, 0.0f), point + new Vector3(0.0f, 2.5f - PlayerStance.RADIUS, 0.0f), PlayerStance.RADIUS, BarricadeManager.checkColliders, RayMasks.BLOCK_STANCE, (QueryTriggerInteraction)1);
                                    for (int index4 = 0; index4 < num; ++index4)
                                    {
                                        if ((UnityEngine.Object)((Component)BarricadeManager.checkColliders[index4]).gameObject != (UnityEngine.Object)interactable.gameObject)
                                            return false;
                                    }
                                    return true;
                                }
                            }
                            else
                                break;
                        }
                    }
                }
            }
            for (ushort index1 = 0; (int)index1 < BarricadeManager.plants.Count; ++index1)
            {
                BarricadeRegion plant = BarricadeManager.plants[(int)index1];
                for (ushort index2 = 0; (int)index2 < plant.barricades.Count; ++index2)
                {
                    if (plant.barricades[(int)index2].barricade.state.Length > 0)
                    {
                        if ((int)index2 < plant.drops.Count)
                        {
                            InteractableBed interactable = plant.drops[(int)index2].interactable as InteractableBed;
                            if ((UnityEngine.Object)interactable != (UnityEngine.Object)null && interactable.owner == owner && Level.checkSafeIncludingClipVolumes(interactable.transform.position))
                            {
                                point = interactable.transform.position;
                                angle = MeasurementTool.angleToByte((float)((int)plant.barricades[(int)index2].angle_y * 2 + 90));
                                int num = Physics.OverlapCapsuleNonAlloc(point + new Vector3(0.0f, PlayerStance.RADIUS, 0.0f), point + new Vector3(0.0f, 2.5f - PlayerStance.RADIUS, 0.0f), PlayerStance.RADIUS, BarricadeManager.checkColliders, RayMasks.BLOCK_STANCE, (QueryTriggerInteraction)1);
                                for (int index3 = 0; index3 < num; ++index3)
                                {
                                    if ((UnityEngine.Object)((Component)BarricadeManager.checkColliders[index3]).gameObject != (UnityEngine.Object)interactable.gameObject)
                                        return false;
                                }
                                return true;
                            }
                        }
                        else
                            break;
                    }
                }
            }
            return false;
        }

        public static void unclaimBeds(CSteamID owner)
        {
            for (byte x = 0; (int)x < (int)Regions.WORLD_SIZE; ++x)
            {
                for (byte y = 0; (int)y < (int)Regions.WORLD_SIZE; ++y)
                {
                    BarricadeRegion region = BarricadeManager.regions[(int)x, (int)y];
                    for (ushort index = 0; (int)index < region.barricades.Count; ++index)
                    {
                        if (region.barricades[(int)index].barricade.state.Length > 0)
                        {
                            if ((int)index < region.drops.Count)
                            {
                                InteractableBed interactable = region.drops[(int)index].interactable as InteractableBed;
                                if ((UnityEngine.Object)interactable != (UnityEngine.Object)null && interactable.owner == owner)
                                {
                                    BarricadeManager.manager.channel.send("tellClaimBed", ESteamCall.ALL, x, y, BarricadeManager.BARRICADE_REGIONS, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)ushort.MaxValue, (object)index, (object)CSteamID.Nil);
                                    BitConverter.GetBytes(interactable.owner.m_SteamID).CopyTo((Array)region.barricades[(int)index].barricade.state, 0);
                                    return;
                                }
                            }
                            else
                                break;
                        }
                    }
                }
            }
            for (ushort index1 = 0; (int)index1 < BarricadeManager.plants.Count; ++index1)
            {
                BarricadeRegion plant = BarricadeManager.plants[(int)index1];
                for (ushort index2 = 0; (int)index2 < plant.barricades.Count; ++index2)
                {
                    if (plant.barricades[(int)index2].barricade.state.Length > 0)
                    {
                        if ((int)index2 < plant.drops.Count)
                        {
                            InteractableBed interactable = plant.drops[(int)index2].interactable as InteractableBed;
                            if ((UnityEngine.Object)interactable != (UnityEngine.Object)null && interactable.owner == owner)
                            {
                                BarricadeManager.manager.channel.send("tellClaimBed", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)byte.MaxValue, (object)byte.MaxValue, (object)index1, (object)index2, (object)CSteamID.Nil);
                                BitConverter.GetBytes(interactable.owner.m_SteamID).CopyTo((Array)plant.barricades[(int)index2].barricade.state, 0);
                                return;
                            }
                        }
                        else
                            break;
                    }
                }
            }
        }

        public static void claimBed(Transform barricade)
        {
            byte x;
            byte y;
            ushort plant;
            ushort index;
            BarricadeRegion region;
            if (!BarricadeManager.tryGetInfo(barricade, out x, out y, out plant, out index, out region))
                return;
            BarricadeManager.manager.channel.send("askClaimBed", ESteamCall.SERVER, ESteamPacket.UPDATE_UNRELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index);
        }

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public void tellClaimBed(
          CSteamID steamID,
          byte x,
          byte y,
          ushort plant,
          ushort index,
          CSteamID owner)
        {
            BarricadeRegion region;
            if (!this.channel.checkServer(steamID) || !BarricadeManager.tryGetRegion(x, y, plant, out region) || !Provider.isServer && !region.isNetworked || (int)index >= region.drops.Count)
                return;
            InteractableBed interactable = region.drops[(int)index].interactable as InteractableBed;
            if (!((UnityEngine.Object)interactable != (UnityEngine.Object)null))
                return;
            interactable.updateClaim(owner);
        }

        [SteamCall(ESteamCallValidation.SERVERSIDE)]
        public void askClaimBed(CSteamID steamID, byte x, byte y, ushort plant, ushort index)
        {
            BarricadeRegion region;
            if (!Provider.isServer || !BarricadeManager.tryGetRegion(x, y, plant, out region))
                return;
            Player player = PlayerTool.getPlayer(steamID);
            if ((UnityEngine.Object)player == (UnityEngine.Object)null || player.life.isDead || ((int)index >= region.drops.Count || !player.tryToPerformRateLimitedAction()) || (double)(region.drops[(int)index].model.transform.position - player.transform.position).sqrMagnitude > 400.0)
                return;
            InteractableBed interactable = region.drops[(int)index].interactable as InteractableBed;
            if (!((UnityEngine.Object)interactable != (UnityEngine.Object)null) || !interactable.isClaimable || !interactable.checkClaim(player.channel.owner.playerID.steamID))
                return;
            if (interactable.isClaimed)
            {
                if (plant == ushort.MaxValue)
                    BarricadeManager.manager.channel.send("tellClaimBed", ESteamCall.ALL, x, y, BarricadeManager.BARRICADE_REGIONS, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)CSteamID.Nil);
                else
                    BarricadeManager.manager.channel.send("tellClaimBed", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)CSteamID.Nil);
            }
            else
            {
                BarricadeManager.unclaimBeds(player.channel.owner.playerID.steamID);
                if (plant == ushort.MaxValue)
                    BarricadeManager.manager.channel.send("tellClaimBed", ESteamCall.ALL, x, y, BarricadeManager.BARRICADE_REGIONS, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)player.channel.owner.playerID.steamID);
                else
                    BarricadeManager.manager.channel.send("tellClaimBed", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)player.channel.owner.playerID.steamID);
            }
            BitConverter.GetBytes(interactable.owner.m_SteamID).CopyTo((Array)region.barricades[(int)index].barricade.state, 0);
        }

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public void tellShootSentry(CSteamID steamID, byte x, byte y, ushort plant, ushort index)
        {
            BarricadeRegion region;
            if (!this.channel.checkServer(steamID) || !BarricadeManager.tryGetRegion(x, y, plant, out region) || !Provider.isServer && !region.isNetworked || (int)index >= region.drops.Count)
                return;
            InteractableSentry interactable = region.drops[(int)index].interactable as InteractableSentry;
            if (!((UnityEngine.Object)interactable != (UnityEngine.Object)null))
                return;
            interactable.shoot();
        }

        public static void sendShootSentry(Transform barricade)
        {
            byte x;
            byte y;
            ushort plant;
            ushort index;
            BarricadeRegion region;
            if (!BarricadeManager.tryGetInfo(barricade, out x, out y, out plant, out index, out region))
                return;
            if (plant == ushort.MaxValue)
                BarricadeManager.manager.channel.send("tellShootSentry", ESteamCall.ALL, x, y, BarricadeManager.BARRICADE_REGIONS, ESteamPacket.UPDATE_UNRELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index);
            else
                BarricadeManager.manager.channel.send("tellShootSentry", ESteamCall.ALL, ESteamPacket.UPDATE_UNRELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index);
        }

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public void tellAlertSentry(
          CSteamID steamID,
          byte x,
          byte y,
          ushort plant,
          ushort index,
          byte yaw,
          byte pitch)
        {
            BarricadeRegion region;
            if (!this.channel.checkServer(steamID) || !BarricadeManager.tryGetRegion(x, y, plant, out region) || !Provider.isServer && !region.isNetworked || (int)index >= region.drops.Count)
                return;
            InteractableSentry interactable = region.drops[(int)index].interactable as InteractableSentry;
            if (!((UnityEngine.Object)interactable != (UnityEngine.Object)null))
                return;
            interactable.alert(MeasurementTool.byteToAngle(yaw), MeasurementTool.byteToAngle(pitch));
        }

        public static void sendAlertSentry(Transform barricade, float yaw, float pitch)
        {
            byte x;
            byte y;
            ushort plant;
            ushort index;
            BarricadeRegion region;
            if (!BarricadeManager.tryGetInfo(barricade, out x, out y, out plant, out index, out region))
                return;
            if (plant == ushort.MaxValue)
                BarricadeManager.manager.channel.send("tellAlertSentry", ESteamCall.ALL, x, y, BarricadeManager.BARRICADE_REGIONS, ESteamPacket.UPDATE_UNRELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)MeasurementTool.angleToByte(yaw), (object)MeasurementTool.angleToByte(pitch));
            else
                BarricadeManager.manager.channel.send("tellAlertSentry", ESteamCall.ALL, ESteamPacket.UPDATE_UNRELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)MeasurementTool.angleToByte(yaw), (object)MeasurementTool.angleToByte(pitch));
        }

        //public static void damage(
        //  Transform barricade,
        //  float damage,
        //  float times,
        //  bool armor,
        //  CSteamID instigatorSteamID = default(CSteamID),
        //  EDamageOrigin damageOrigin = EDamageOrigin.Unknown)
        //{
        //    byte x;
        //    byte y;
        //    ushort plant;
        //    ushort index;
        //    BarricadeRegion region;
        //    if (!BarricadeManager.tryGetInfo(barricade, out x, out y, out plant, out index, out region) || region.barricades[(int)index].barricade.isDead)
        //        return;
        //    ItemBarricadeAsset itemBarricadeAsset = (ItemBarricadeAsset)Assets.find(EAssetType.ITEM, region.barricades[(int)index].barricade.id);
        //    if (itemBarricadeAsset == null)
        //        return;
        //    if (armor)
        //        times *= Provider.modeConfigData.Barricades.getArmorMultiplier(itemBarricadeAsset.armorTier);
        //    ushort pendingTotalDamage = (ushort)((double)damage * (double)times);
        //    bool shouldAllow = true;
        //    if (BarricadeManager.onDamageBarricadeRequested != null)
        //        BarricadeManager.onDamageBarricadeRequested(instigatorSteamID, barricade, ref pendingTotalDamage, ref shouldAllow, damageOrigin);
        //    if (!shouldAllow || pendingTotalDamage < (ushort)1)
        //        return;
        //    region.barricades[(int)index].barricade.askDamage(pendingTotalDamage);
        //    if (region.barricades[(int)index].barricade.isDead)
        //    {
        //        if (itemBarricadeAsset.explosion != (ushort)0)
        //        {
        //            if (plant == ushort.MaxValue)
        //                EffectManager.sendEffect(itemBarricadeAsset.explosion, x, y, BarricadeManager.BARRICADE_REGIONS, barricade.position + Vector3.down * itemBarricadeAsset.offset);
        //            else
        //                EffectManager.sendEffect(itemBarricadeAsset.explosion, EffectManager.MEDIUM, barricade.position + Vector3.down * itemBarricadeAsset.offset);
        //        }
        //        region.barricades.RemoveAt((int)index);
        //        if (plant == ushort.MaxValue)
        //            BarricadeManager.manager.channel.send("tellTakeBarricade", ESteamCall.ALL, x, y, BarricadeManager.BARRICADE_REGIONS, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index);
        //        else
        //            BarricadeManager.manager.channel.send("tellTakeBarricade", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index);
        //    }
        //    else
        //        BarricadeManager.sendHealthChanged(x, y, plant, index, region);
        //}

        //private static void sendHealthChanged(
        //  byte x,
        //  byte y,
        //  ushort plant,
        //  ushort index,
        //  BarricadeRegion region)
        //{
        //    for (int index1 = 0; index1 < Provider.clients.Count; ++index1)
        //    {
        //        if (OwnershipTool.checkToggle(Provider.clients[index1].playerID.steamID, region.barricades[(int)index].owner, Provider.clients[index1].player.quests.groupID, region.barricades[(int)index].group))
        //        {
        //            if (plant == ushort.MaxValue)
        //            {
        //                if ((UnityEngine.Object)Provider.clients[index1].player != (UnityEngine.Object)null && Regions.checkArea(x, y, Provider.clients[index1].player.movement.region_x, Provider.clients[index1].player.movement.region_y, BarricadeManager.BARRICADE_REGIONS))
        //                    BarricadeManager.manager.channel.send("tellBarricadeHealth", Provider.clients[index1].playerID.steamID, ESteamPacket.UPDATE_UNRELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)(byte)((double)region.barricades[(int)index].barricade.health / (double)region.barricades[(int)index].barricade.asset.health * 100.0));
        //            }
        //            else
        //                BarricadeManager.manager.channel.send("tellBarricadeHealth", Provider.clients[index1].playerID.steamID, ESteamPacket.UPDATE_UNRELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index, (object)(byte)((double)region.barricades[(int)index].barricade.health / (double)region.barricades[(int)index].barricade.asset.health * 100.0));
        //        }
        //    }
        //}

        //public static void repair(Transform barricade, float damage, float times)
        //{
        //    byte x;
        //    byte y;
        //    ushort plant;
        //    ushort index1;
        //    BarricadeRegion region;
        //    if (!BarricadeManager.tryGetInfo(barricade, out x, out y, out plant, out index1, out region) || region.barricades[(int)index1].barricade.isDead || region.barricades[(int)index1].barricade.isRepaired)
        //        return;
        //    ushort amount = (ushort)((double)damage * (double)times);
        //    region.barricades[(int)index1].barricade.askRepair(amount);
        //    for (int index2 = 0; index2 < Provider.clients.Count; ++index2)
        //    {
        //        if (OwnershipTool.checkToggle(Provider.clients[index2].playerID.steamID, region.barricades[(int)index1].owner, Provider.clients[index2].player.quests.groupID, region.barricades[(int)index1].group))
        //        {
        //            if (plant == ushort.MaxValue)
        //            {
        //                if ((UnityEngine.Object)Provider.clients[index2].player != (UnityEngine.Object)null && Regions.checkArea(x, y, Provider.clients[index2].player.movement.region_x, Provider.clients[index2].player.movement.region_y, BarricadeManager.BARRICADE_REGIONS))
        //                    BarricadeManager.manager.channel.send("tellBarricadeHealth", Provider.clients[index2].playerID.steamID, ESteamPacket.UPDATE_UNRELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index1, (object)(byte)Mathf.RoundToInt((float)((double)region.barricades[(int)index1].barricade.health / (double)region.barricades[(int)index1].barricade.asset.health * 100.0)));
        //            }
        //            else
        //                BarricadeManager.manager.channel.send("tellBarricadeHealth", Provider.clients[index2].playerID.steamID, ESteamPacket.UPDATE_UNRELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)index1, (object)(byte)Mathf.RoundToInt((float)((double)region.barricades[(int)index1].barricade.health / (double)region.barricades[(int)index1].barricade.asset.health * 100.0)));
        //        }
        //    }
        //}

        //public static Transform dropBarricade(
        //  Barricade barricade,
        //  Transform hit,
        //  Vector3 point,
        //  float angle_x,
        //  float angle_y,
        //  float angle_z,
        //  ulong owner,
        //  ulong group)
        //{
        //    ItemBarricadeAsset asset = (ItemBarricadeAsset)Assets.find(EAssetType.ITEM, barricade.id);
        //    if (asset == null)
        //        return (Transform)null;
        //    bool shouldAllow = true;
        //    if (BarricadeManager.onDeployBarricadeRequested != null)
        //        BarricadeManager.onDeployBarricadeRequested(barricade, asset, hit, ref point, ref angle_x, ref angle_y, ref angle_z, ref owner, ref group, ref shouldAllow);
        //    if (!shouldAllow)
        //        return (Transform)null;
        //    Transform transform = (Transform)null;
        //    Vector3 eulerAngles = BarricadeManager.getRotation(asset, angle_x, angle_y, angle_z).eulerAngles;
        //    angle_x = (float)(Mathf.RoundToInt(eulerAngles.x / 2f) * 2);
        //    angle_y = (float)(Mathf.RoundToInt(eulerAngles.y / 2f) * 2);
        //    angle_z = (float)(Mathf.RoundToInt(eulerAngles.z / 2f) * 2);
        //    if ((UnityEngine.Object)hit != (UnityEngine.Object)null && hit.transform.CompareTag("Vehicle"))
        //    {
        //        byte x;
        //        byte y;
        //        ushort plant;
        //        BarricadeRegion region;
        //        if (BarricadeManager.tryGetPlant(hit, out x, out y, out plant, out region))
        //        {
        //            BarricadeData barricadeData = new BarricadeData(barricade, point, MeasurementTool.angleToByte(angle_x), MeasurementTool.angleToByte(angle_y), MeasurementTool.angleToByte(angle_z), owner, group, Provider.time);
        //            region.barricades.Add(barricadeData);
        //            uint instanceID = ++BarricadeManager.instanceCount;
        //            transform = BarricadeManager.manager.spawnBarricade(region, barricade.id, barricade.state, barricadeData.point, barricadeData.angle_x, barricadeData.angle_y, barricadeData.angle_z, (byte)100, barricadeData.owner, barricadeData.group, instanceID);
        //            BarricadeManager.manager.channel.send("tellBarricade", ESteamCall.OTHERS, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)plant, (object)barricade.id, (object)barricade.state, (object)barricadeData.point, (object)barricadeData.angle_x, (object)barricadeData.angle_y, (object)barricadeData.angle_z, (object)barricadeData.owner, (object)barricadeData.group, (object)instanceID);
        //        }
        //    }
        //    else
        //    {
        //        byte x;
        //        byte y;
        //        BarricadeRegion region;
        //        if (Regions.tryGetCoordinate(point, out x, out y) && BarricadeManager.tryGetRegion(x, y, ushort.MaxValue, out region))
        //        {
        //            BarricadeData barricadeData = new BarricadeData(barricade, point, MeasurementTool.angleToByte(angle_x), MeasurementTool.angleToByte(angle_y), MeasurementTool.angleToByte(angle_z), owner, group, Provider.time);
        //            region.barricades.Add(barricadeData);
        //            uint instanceID = ++BarricadeManager.instanceCount;
        //            transform = BarricadeManager.manager.spawnBarricade(region, barricade.id, barricade.state, barricadeData.point, barricadeData.angle_x, barricadeData.angle_y, barricadeData.angle_z, (byte)100, barricadeData.owner, barricadeData.group, instanceID);
        //            BarricadeManager.manager.channel.send("tellBarricade", ESteamCall.OTHERS, x, y, BarricadeManager.BARRICADE_REGIONS, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y, (object)ushort.MaxValue, (object)barricade.id, (object)barricade.state, (object)barricadeData.point, (object)barricadeData.angle_x, (object)barricadeData.angle_y, (object)barricadeData.angle_z, (object)barricadeData.owner, (object)barricadeData.group, (object)instanceID);
        //        }
        //    }
        //    return transform;
        //}

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public void tellTakeBarricade(CSteamID steamID, byte x, byte y, ushort plant, ushort index)
        {
            BarricadeRegion region;
            if (!this.channel.checkServer(steamID) || !BarricadeManager.tryGetRegion(x, y, plant, out region) || !Provider.isServer && !region.isNetworked || (int)index >= region.drops.Count)
                return;
            region.drops[(int)index].model.GetComponent<IManualOnDestroy>()?.ManualOnDestroy();
            UnityEngine.Object.Destroy((UnityEngine.Object)region.drops[(int)index].model.gameObject);
            region.drops[(int)index].model.position = Vector3.zero;
            region.drops.RemoveAt((int)index);
        }

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public void tellClearRegionBarricades(CSteamID steamID, byte x, byte y)
        {
            if (!this.channel.checkServer(steamID) || !Provider.isServer && !BarricadeManager.regions[(int)x, (int)y].isNetworked)
                return;
            BarricadeManager.regions[(int)x, (int)y].destroy();
        }

        public static void askClearRegionBarricades(byte x, byte y)
        {
            if (!Provider.isServer || !Regions.checkSafe((int)x, (int)y))
                return;
            BarricadeRegion region = BarricadeManager.regions[(int)x, (int)y];
            if (region.barricades.Count <= 0)
                return;
            region.barricades.Clear();
            BarricadeManager.manager.channel.send("tellClearRegionBarricades", ESteamCall.ALL, x, y, BarricadeManager.BARRICADE_REGIONS, ESteamPacket.UPDATE_RELIABLE_BUFFER, (object)x, (object)y);
        }

        public static void askClearAllBarricades()
        {
            if (!Provider.isServer)
                return;
            for (byte x = 0; (int)x < (int)Regions.WORLD_SIZE; ++x)
            {
                for (byte y = 0; (int)y < (int)Regions.WORLD_SIZE; ++y)
                    BarricadeManager.askClearRegionBarricades(x, y);
            }
        }

        public static Quaternion getRotation(
          ItemBarricadeAsset asset,
          float angle_x,
          float angle_y,
          float angle_z)
        {
            return Quaternion.Euler(0.0f, angle_y, 0.0f) * Quaternion.Euler((asset.build == EBuild.DOOR || asset.build == EBuild.GATE || (asset.build == EBuild.SHUTTER || asset.build == EBuild.HATCH) ? 0.0f : -90f) + angle_x, 0.0f, 0.0f) * Quaternion.Euler(0.0f, angle_z, 0.0f);
        }

        //private Transform spawnBarricade(
        //  BarricadeRegion region,
        //  ushort id,
        //  byte[] state,
        //  Vector3 point,
        //  byte angle_x,
        //  byte angle_y,
        //  byte angle_z,
        //  byte hp,
        //  ulong owner,
        //  ulong group,
        //  uint instanceID)
        //{
        //    if (id == (ushort)0)
        //        return (Transform)null;
        //    Asset asset1 = Assets.find(EAssetType.ITEM, id);
        //    if (asset1 == null)
        //    {
        //        if (!Provider.isServer)
        //        {
        //            Assets.reportError(string.Format("Missing barricade ID {0}, must disconnect", (object)id));
        //            Provider.connectionFailureInfo = ESteamConnectionFailureInfo.BARRICADE;
        //            Provider.connectionFailureReason = id.ToString();
        //            Provider.disconnect();
        //        }
        //        return (Transform)null;
        //    }
        //    Transform newModel = (Transform)null;
        //    try
        //    {
        //        ItemBarricadeAsset asset2 = asset1 as ItemBarricadeAsset;
        //        newModel = BarricadeTool.getBarricade(region.parent, hp, owner, group, point, Quaternion.Euler((float)((int)angle_x * 2), (float)((int)angle_y * 2), (float)((int)angle_z * 2)), id, state, asset2);
        //        BarricadeManager.barricadeColliders.Clear();
        //        newModel.GetComponentsInChildren<Collider>(BarricadeManager.barricadeColliders);
        //        if ((UnityEngine.Object)region.parent != (UnityEngine.Object)LevelBarricades.models)
        //        {
        //            for (int index = 0; index < BarricadeManager.barricadeColliders.Count; ++index)
        //            {
        //                if (BarricadeManager.barricadeColliders[index] is MeshCollider)
        //                    BarricadeManager.barricadeColliders[index].set_enabled(false);
        //                if ((UnityEngine.Object)((Component)BarricadeManager.barricadeColliders[index]).GetComponent<Rigidbody>() == (UnityEngine.Object)null)
        //                {
        //                    Rigidbody rigidbody = ((Component)BarricadeManager.barricadeColliders[index]).gameObject.AddComponent<Rigidbody>();
        //                    rigidbody.set_useGravity(false);
        //                    rigidbody.set_isKinematic(true);
        //                }
        //                if (BarricadeManager.barricadeColliders[index] is MeshCollider)
        //                    BarricadeManager.barricadeColliders[index].set_enabled(true);
        //            }
        //        }
        //        if ((UnityEngine.Object)region.parent != (UnityEngine.Object)LevelBarricades.models)
        //        {
        //            newModel.gameObject.SetActive(false);
        //            newModel.gameObject.SetActive(true);
        //            BarricadeManager.getVehicleColliders(region.parent);
        //            for (int index1 = 0; index1 < BarricadeManager.barricadeColliders.Count; ++index1)
        //            {
        //                if (((Component)BarricadeManager.barricadeColliders[index1]).gameObject.layer == LayerMasks.BARRICADE)
        //                    ((Component)BarricadeManager.barricadeColliders[index1]).gameObject.layer = LayerMasks.RESOURCE;
        //                for (int index2 = 0; index2 < BarricadeManager.vehicleColliders.Count; ++index2)
        //                    Physics.IgnoreCollision(BarricadeManager.vehicleColliders[index2], BarricadeManager.barricadeColliders[index1], true);
        //            }
        //        }
        //        BarricadeDrop drop = new BarricadeDrop(newModel, newModel.GetComponent<Interactable>(), instanceID);
        //        region.drops.Add(drop);
        //        if (BarricadeManager.onBarricadeSpawned != null)
        //            BarricadeManager.onBarricadeSpawned(region, drop);
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.LogWarningFormat("Exception while spawning barricade: {0}", (object)id);
        //        Debug.LogException(ex);
        //    }
        //    return newModel;
        //}

        //[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        //public void tellBarricade(
        //  CSteamID steamID,
        //  byte x,
        //  byte y,
        //  ushort plant,
        //  ushort id,
        //  byte[] state,
        //  Vector3 point,
        //  byte angle_x,
        //  byte angle_y,
        //  byte angle_z,
        //  ulong owner,
        //  ulong group,
        //  uint instanceID)
        //{
        //    BarricadeRegion region;
        //    if (!this.channel.checkServer(steamID) || !BarricadeManager.tryGetRegion(x, y, plant, out region) || !Provider.isServer && !region.isNetworked)
        //        return;
        //    this.spawnBarricade(region, id, state, point, angle_x, angle_y, angle_z, (byte)100, owner, group, instanceID);
        //}

        //[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        //public void tellBarricades(CSteamID steamID)
        //{
        //    if (!this.channel.checkServer(steamID))
        //        return;
        //    BarricadeRegion region;
        //    if (BarricadeManager.tryGetRegion((byte)this.channel.read(Types.BYTE_TYPE), (byte)this.channel.read(Types.BYTE_TYPE), (ushort)this.channel.read(Types.UINT16_TYPE), out region))
        //    {
        //        if ((byte)this.channel.read(Types.BYTE_TYPE) == (byte)0)
        //        {
        //            if (region.isNetworked)
        //                return;
        //        }
        //        else if (!region.isNetworked)
        //            return;
        //        region.isNetworked = true;
        //        ushort num = (ushort)this.channel.read(Types.UINT16_TYPE);
        //        for (int index = 0; index < (int)num; ++index)
        //        {
        //            object[] objArray = this.channel.read(Types.UINT16_TYPE, Types.BYTE_ARRAY_TYPE, Types.VECTOR3_TYPE, Types.BYTE_TYPE, Types.BYTE_TYPE, Types.BYTE_TYPE, Types.UINT64_TYPE, Types.UINT64_TYPE, Types.UINT32_TYPE);
        //            ulong owner = (ulong)objArray[6];
        //            ulong group = (ulong)objArray[7];
        //            uint instanceID = (uint)objArray[8];
        //            byte hp = (byte)this.channel.read(Types.BYTE_TYPE);
        //            this.spawnBarricade(region, (ushort)objArray[0], (byte[])objArray[1], (Vector3)objArray[2], (byte)objArray[3], (byte)objArray[4], (byte)objArray[5], hp, owner, group, instanceID);
        //        }
        //    }
        //    Level.isLoadingBarricades = false;
        //}

        public void askBarricades(CSteamID steamID, byte x, byte y, ushort plant)
        {
            BarricadeRegion region;
            if (!BarricadeManager.tryGetRegion(x, y, plant, out region))
                return;
            if (region.barricades.Count > 0 && region.drops.Count == region.barricades.Count)
            {
                byte num1 = 0;
                int index1 = 0;
                int index2 = 0;
                while (index1 < region.barricades.Count)
                {
                    int num2 = 0;
                    while (index2 < region.barricades.Count)
                    {
                        num2 += 38 + region.barricades[index2].barricade.state.Length;
                        ++index2;
                        if (num2 > Block.BUFFER_SIZE / 2)
                            break;
                    }
                    this.channel.openWrite();
                    this.channel.write((object)x);
                    this.channel.write((object)y);
                    this.channel.write((object)plant);
                    this.channel.write((object)num1);
                    this.channel.write((object)(ushort)(index2 - index1));
                    for (; index1 < index2; ++index1)
                    {
                        BarricadeData barricade = region.barricades[index1];
                        InteractableStorage interactable = region.drops[index1].interactable as InteractableStorage;
                        if ((UnityEngine.Object)interactable != (UnityEngine.Object)null)
                        {
                            byte[] numArray;
                            if (interactable.isDisplay)
                            {
                                byte[] bytes1 = Encoding.UTF8.GetBytes(interactable.displayTags);
                                byte[] bytes2 = Encoding.UTF8.GetBytes(interactable.displayDynamicProps);
                                numArray = new byte[20 + (interactable.displayItem == null ? 0 : interactable.displayItem.state.Length) + 4 + 1 + bytes1.Length + 1 + bytes2.Length + 1];
                                if (interactable.displayItem != null)
                                {
                                    Array.Copy((Array)BitConverter.GetBytes(interactable.displayItem.id), 0, (Array)numArray, 16, 2);
                                    numArray[18] = interactable.displayItem.quality;
                                    numArray[19] = (byte)interactable.displayItem.state.Length;
                                    Array.Copy((Array)interactable.displayItem.state, 0, (Array)numArray, 20, interactable.displayItem.state.Length);
                                    Array.Copy((Array)BitConverter.GetBytes(interactable.displaySkin), 0, (Array)numArray, 20 + interactable.displayItem.state.Length, 2);
                                    Array.Copy((Array)BitConverter.GetBytes(interactable.displayMythic), 0, (Array)numArray, 20 + interactable.displayItem.state.Length + 2, 2);
                                    numArray[20 + interactable.displayItem.state.Length + 4] = (byte)bytes1.Length;
                                    Array.Copy((Array)bytes1, 0, (Array)numArray, 20 + interactable.displayItem.state.Length + 5, bytes1.Length);
                                    numArray[20 + interactable.displayItem.state.Length + 5 + bytes1.Length] = (byte)bytes2.Length;
                                    Array.Copy((Array)bytes2, 0, (Array)numArray, 20 + interactable.displayItem.state.Length + 5 + bytes1.Length + 1, bytes2.Length);
                                    numArray[20 + interactable.displayItem.state.Length + 5 + bytes1.Length + 1 + bytes2.Length] = interactable.rot_comp;
                                }
                            }
                            else
                                numArray = new byte[16];
                            Array.Copy((Array)barricade.barricade.state, 0, (Array)numArray, 0, 16);
                            this.channel.write((object)barricade.barricade.id, (object)numArray, (object)barricade.point, (object)barricade.angle_x, (object)barricade.angle_y, (object)barricade.angle_z, (object)barricade.owner, (object)barricade.group, (object)region.drops[index1].instanceID);
                        }
                        else
                            this.channel.write((object)barricade.barricade.id, (object)barricade.barricade.state, (object)barricade.point, (object)barricade.angle_x, (object)barricade.angle_y, (object)barricade.angle_z, (object)barricade.owner, (object)barricade.group, (object)region.drops[index1].instanceID);
                        this.channel.write((object)(byte)Mathf.RoundToInt((float)((double)barricade.barricade.health / (double)barricade.barricade.asset.health * 100.0)));
                    }
                    this.channel.closeWrite("tellBarricades", steamID, ESteamPacket.UPDATE_RELIABLE_CHUNK_BUFFER);
                    ++num1;
                }
            }
            else
            {
                this.channel.openWrite();
                this.channel.write((object)x);
                this.channel.write((object)y);
                this.channel.write((object)plant);
                this.channel.write((object)(byte)0);
                this.channel.write((object)(ushort)0);
                this.channel.closeWrite("tellBarricades", steamID, ESteamPacket.UPDATE_RELIABLE_CHUNK_BUFFER);
            }
        }

        public static void clearPlants()
        {
            BarricadeManager.plants = new List<BarricadeRegion>();
        }

        public static void waterPlant(Transform parent)
        {
            BarricadeManager.plants.Add(new BarricadeRegion(parent)
            {
                isNetworked = false
            });
        }

        public static void uprootPlant(Transform parent)
        {
            for (ushort index = 0; (int)index < BarricadeManager.plants.Count; ++index)
            {
                BarricadeRegion plant = BarricadeManager.plants[(int)index];
                if ((UnityEngine.Object)plant.parent == (UnityEngine.Object)parent)
                {
                    plant.barricades.Clear();
                    plant.destroy();
                    BarricadeManager.plants.RemoveAt((int)index);
                    break;
                }
            }
        }

        public static void trimPlant(Transform parent)
        {
            for (ushort index = 0; (int)index < BarricadeManager.plants.Count; ++index)
            {
                BarricadeRegion plant = BarricadeManager.plants[(int)index];
                if ((UnityEngine.Object)plant.parent == (UnityEngine.Object)parent)
                {
                    plant.barricades.Clear();
                    plant.destroy();
                    break;
                }
            }
        }

        public static void askPlants(CSteamID steamID)
        {
            for (ushort plant = 0; (int)plant < BarricadeManager.plants.Count; ++plant)
                BarricadeManager.manager.askBarricades(steamID, byte.MaxValue, byte.MaxValue, plant);
        }

        public static void askPlants(Transform parent)
        {
            byte x;
            byte y;
            ushort plant;
            BarricadeRegion region;
            if (!BarricadeManager.tryGetPlant(parent, out x, out y, out plant, out region))
                return;
            BarricadeManager.manager.channel.openWrite();
            BarricadeManager.manager.channel.write((object)x);
            BarricadeManager.manager.channel.write((object)y);
            BarricadeManager.manager.channel.write((object)plant);
            BarricadeManager.manager.channel.write((object)(byte)0);
            BarricadeManager.manager.channel.write((object)(ushort)0);
            BarricadeManager.manager.channel.closeWrite("tellBarricades", ESteamCall.OTHERS, ESteamPacket.UPDATE_RELIABLE_CHUNK_BUFFER);
        }

        public static bool tryGetInfo(
          Transform barricade,
          out byte x,
          out byte y,
          out ushort plant,
          out ushort index,
          out BarricadeRegion region)
        {
            x = (byte)0;
            y = (byte)0;
            plant = (ushort)0;
            index = (ushort)0;
            region = (BarricadeRegion)null;
            if (BarricadeManager.tryGetRegion(barricade, out x, out y, out plant, out region))
            {
                index = (ushort)0;
                while ((int)index < region.drops.Count)
                {
                    if ((UnityEngine.Object)barricade == (UnityEngine.Object)region.drops[(int)index].model)
                        return true;
                    ++index;
                }
            }
            return false;
        }

        public static bool tryGetInfo(
          Transform barricade,
          out byte x,
          out byte y,
          out ushort plant,
          out ushort index,
          out BarricadeRegion region,
          out BarricadeDrop drop)
        {
            x = (byte)0;
            y = (byte)0;
            plant = (ushort)0;
            index = (ushort)0;
            region = (BarricadeRegion)null;
            drop = (BarricadeDrop)null;
            if (BarricadeManager.tryGetRegion(barricade, out x, out y, out plant, out region))
            {
                index = (ushort)0;
                while ((int)index < region.drops.Count)
                {
                    if ((UnityEngine.Object)barricade == (UnityEngine.Object)region.drops[(int)index].model)
                    {
                        drop = region.drops[(int)index];
                        return true;
                    }
                    ++index;
                }
            }
            return false;
        }

        public static bool tryGetPlant(
          Transform parent,
          out byte x,
          out byte y,
          out ushort plant,
          out BarricadeRegion region)
        {
            x = byte.MaxValue;
            y = byte.MaxValue;
            plant = ushort.MaxValue;
            region = (BarricadeRegion)null;
            if ((UnityEngine.Object)parent == (UnityEngine.Object)null)
                return false;
            plant = (ushort)0;
            while ((int)plant < BarricadeManager.plants.Count)
            {
                region = BarricadeManager.plants[(int)plant];
                if ((UnityEngine.Object)region.parent == (UnityEngine.Object)parent)
                    return true;
                ++plant;
            }
            return false;
        }

        public static bool tryGetRegion(
          Transform barricade,
          out byte x,
          out byte y,
          out ushort plant,
          out BarricadeRegion region)
        {
            x = (byte)0;
            y = (byte)0;
            plant = (ushort)0;
            region = (BarricadeRegion)null;
            if ((UnityEngine.Object)barricade == (UnityEngine.Object)null)
                return false;
            if (barricade.parent.CompareTag("Vehicle"))
            {
                plant = (ushort)0;
                while ((int)plant < BarricadeManager.plants.Count)
                {
                    region = BarricadeManager.plants[(int)plant];
                    if ((UnityEngine.Object)region.parent == (UnityEngine.Object)barricade.parent)
                        return true;
                    ++plant;
                }
            }
            else
            {
                plant = ushort.MaxValue;
                if (Regions.tryGetCoordinate(barricade.position, out x, out y))
                {
                    region = BarricadeManager.regions[(int)x, (int)y];
                    return true;
                }
            }
            return false;
        }

        public static InteractableVehicle getVehicleFromPlant(ushort plant)
        {
            if ((int)plant < BarricadeManager.plants.Count)
                return DamageTool.getVehicle(BarricadeManager.plants[(int)plant].parent);
            return (InteractableVehicle)null;
        }

        public static BarricadeRegion getRegionFromVehicle(InteractableVehicle vehicle)
        {
            if ((UnityEngine.Object)vehicle == (UnityEngine.Object)null)
                return (BarricadeRegion)null;
            Transform transform = vehicle.transform;
            foreach (BarricadeRegion plant in BarricadeManager.plants)
            {
                if ((UnityEngine.Object)plant.parent == (UnityEngine.Object)transform)
                    return plant;
            }
            return (BarricadeRegion)null;
        }

        public static bool tryGetRegion(byte x, byte y, ushort plant, out BarricadeRegion region)
        {
            region = (BarricadeRegion)null;
            if (plant < ushort.MaxValue)
            {
                if ((int)plant >= BarricadeManager.plants.Count)
                    return false;
                region = BarricadeManager.plants[(int)plant];
                return true;
            }
            if (!Regions.checkSafe((int)x, (int)y))
                return false;
            region = BarricadeManager.regions[(int)x, (int)y];
            return true;
        }

        //public static void updateState(Transform barricade, byte[] state, int size)
        //{
        //    byte x;
        //    byte y;
        //    ushort plant;
        //    ushort index;
        //    BarricadeRegion region;
        //    if (!BarricadeManager.tryGetInfo(barricade, out x, out y, out plant, out index, out region))
        //        return;
        //    if (region.barricades[(int)index].barricade.state.Length != size)
        //        region.barricades[(int)index].barricade.state = new byte[size];
        //    Array.Copy((Array)state, (Array)region.barricades[(int)index].barricade.state, size);
        //}

        //private static void updateActivity(BarricadeRegion region, CSteamID owner, CSteamID group)
        //{
        //    for (ushort index = 0; (int)index < region.barricades.Count; ++index)
        //    {
        //        BarricadeData barricade = region.barricades[(int)index];
        //        if (OwnershipTool.checkToggle(owner, barricade.owner, group, barricade.group))
        //            barricade.objActiveDate = Provider.time;
        //    }
        //}

        //private static void updateActivity(CSteamID owner, CSteamID group)
        //{
        //    for (byte index1 = 0; (int)index1 < (int)Regions.WORLD_SIZE; ++index1)
        //    {
        //        for (byte index2 = 0; (int)index2 < (int)Regions.WORLD_SIZE; ++index2)
        //            BarricadeManager.updateActivity(BarricadeManager.regions[(int)index1, (int)index2], owner, group);
        //    }
        //    for (ushort index = 0; (int)index < BarricadeManager.plants.Count; ++index)
        //        BarricadeManager.updateActivity(BarricadeManager.plants[(int)index], owner, group);
        //}

        //private void onLevelLoaded(int level)
        //{
        //    if (level <= Level.BUILD_INDEX_SETUP)
        //        return;
        //    BarricadeManager.regions = new BarricadeRegion[(int)Regions.WORLD_SIZE, (int)Regions.WORLD_SIZE];
        //    for (byte index1 = 0; (int)index1 < (int)Regions.WORLD_SIZE; ++index1)
        //    {
        //        for (byte index2 = 0; (int)index2 < (int)Regions.WORLD_SIZE; ++index2)
        //            BarricadeManager.regions[(int)index1, (int)index2] = new BarricadeRegion(LevelBarricades.models);
        //    }
        //    BarricadeManager.barricadeColliders = new List<Collider>();
        //    BarricadeManager.vehicleColliders = new List<Collider>();
        //    BarricadeManager.vehicleSubColliders = new List<Collider>();
        //    BarricadeManager.version = BarricadeManager.SAVEDATA_VERSION;
        //    BarricadeManager.instanceCount = 0U;
        //    if (!Provider.isServer)
        //        return;
        //    BarricadeManager.load();
        //}

        private void onRegionUpdated(
          Player player,
          byte old_x,
          byte old_y,
          byte new_x,
          byte new_y,
          byte step,
          ref bool canIncrementIndex)
        {
            if (step == (byte)0)
            {
                for (byte x_0 = 0; (int)x_0 < (int)Regions.WORLD_SIZE; ++x_0)
                {
                    for (byte y_0 = 0; (int)y_0 < (int)Regions.WORLD_SIZE; ++y_0)
                    {
                        if (Provider.isServer)
                        {
                            if (player.movement.loadedRegions[(int)x_0, (int)y_0].isBarricadesLoaded && !Regions.checkArea(x_0, y_0, new_x, new_y, BarricadeManager.BARRICADE_REGIONS))
                                player.movement.loadedRegions[(int)x_0, (int)y_0].isBarricadesLoaded = false;
                        }
                        else if (player.channel.isOwner && BarricadeManager.regions[(int)x_0, (int)y_0].isNetworked && !Regions.checkArea(x_0, y_0, new_x, new_y, BarricadeManager.BARRICADE_REGIONS))
                        {
                            BarricadeManager.regions[(int)x_0, (int)y_0].destroy();
                            BarricadeManager.regions[(int)x_0, (int)y_0].isNetworked = false;
                        }
                    }
                }
            }
            if (step != (byte)2 || !Dedicator.isDedicated || !Regions.checkSafe((int)new_x, (int)new_y))
                return;
            for (int index1 = (int)new_x - (int)BarricadeManager.BARRICADE_REGIONS; index1 <= (int)new_x + (int)BarricadeManager.BARRICADE_REGIONS; ++index1)
            {
                for (int index2 = (int)new_y - (int)BarricadeManager.BARRICADE_REGIONS; index2 <= (int)new_y + (int)BarricadeManager.BARRICADE_REGIONS; ++index2)
                {
                    if (Regions.checkSafe((int)(byte)index1, (int)(byte)index2) && !player.movement.loadedRegions[index1, index2].isBarricadesLoaded)
                    {
                        player.movement.loadedRegions[index1, index2].isBarricadesLoaded = true;
                        this.askBarricades(player.channel.owner.playerID.steamID, (byte)index1, (byte)index2, ushort.MaxValue);
                    }
                }
            }
        }

        //private void onPlayerCreated(Player player)
        //{
        //    player.movement.onRegionUpdated += new PlayerRegionUpdated(this.onRegionUpdated);
        //    if (!Provider.isServer)
        //        return;
        //    BarricadeManager.updateActivity(player.channel.owner.playerID.steamID, player.quests.groupID);
        //}

        //private void Start()
        //{
        //    BarricadeManager.manager = this;
        //    Level.onPreLevelLoaded += new PreLevelLoaded(this.onLevelLoaded);
        //    Player.onPlayerCreated += new PlayerCreated(this.onPlayerCreated);
        //}
    }
}
