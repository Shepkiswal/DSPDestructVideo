using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace DSPDestructVideo
{

    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInProcess(GAME_PROCESS)]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "com.lin.dps.destructVideo";
        public const string NAME = "DSPDestructVideo";
        public const string VERSION = "1.0";
        public const string GAME_VERSION = "0.6.15.5706";
        public const string GAME_PROCESS = "DSPGAME.exe";

        /// <summary>
        /// 需要的所有传送带节点，第一次调用时赋值
        /// </summary>
        public static List<EntityData> loadBeltList = new List<EntityData>();

        /// <summary>
        /// Unity声明周期，开始，Patch游戏和载入字符帧数据
        /// </summary>
        void Start()
        {
            Harmony.CreateAndPatchAll(typeof(Plugin));
            VideoChars.LoadFrameData();
        }

        /// <summary>
        /// 根据传送带id获取传送带线路
        /// </summary>
        public static CargoPath GetPathByBeltId(PlanetFactory factory, int beltId)
        {
            foreach (var path in factory.cargoTraffic.pathPool)
            {
                if (path == null) continue;
                if (path.belts.Contains(beltId))
                {
                    return path;
                }
            }
            return null;
        }

        /// <summary>
        /// 根据线路获取所有传送带
        /// </summary>
        public static List<EntityData> GetBletsByPath(PlanetFactory factory, CargoPath path)
        {
            List<EntityData> beltList = new List<EntityData>();
            foreach (var beltId in path.belts)
            {
                foreach (var entity in factory.entityPool)
                {
                    if (beltId == entity.beltId)
                    {
                        beltList.Add(entity);
                    }
                }
            }
            return beltList;
        }

        /// <summary>
        /// 根据物品描述获取实体
        /// </summary>
        public static List<EntityData> GetEntitysByProto(PlanetFactory factory, ItemProto itemProto)
        {
            List<EntityData> beltList = new List<EntityData>();
            foreach (var entity in factory.entityPool)
            {
                if (entity.id != 0)
                {
                    if (entity.protoId == itemProto.ID)
                    {
                         if ((entity.pos - GameMain.data.mainPlayer.position).sqrMagnitude <= GameMain.data.mainPlayer.mecha.buildArea * GameMain.data.mainPlayer.mecha.buildArea)
                         {
                         beltList.Add(entity);
                         }
                    }
                }
            }
            return beltList;
        }

        /// <summary>
        /// 根据传送带List控制红x的显示，大部分都是游戏内原本的代码
        /// </summary>
        public static void CustomDestructPreviews(PlayerAction_Build __instance, List<EntityData> beltList)
        {
            var _this = __instance;
            PlanetFactory factory = Traverse.Create(_this).Field("factory").GetValue<PlanetFactory>();
            
            for (int i = 0; i < beltList.Count; i++)
            {
                _this.AddBuildPreview(new BuildPreview());
            }

            for (int i = 0; i < _this.buildPreviews.Count; i++)
            {
                BuildPreview buildPreview = _this.buildPreviews[i];
                ItemProto proto = Traverse.Create(_this).Method("GetItemProto", beltList[i].id).GetValue<ItemProto>();
                buildPreview.item = proto;
                buildPreview.desc = proto.prefabDesc;
                buildPreview.lpos = beltList[i].pos;
                buildPreview.lrot = beltList[i].rot;
                buildPreview.objId = beltList[i].id;


                if (buildPreview.desc.lodCount > 0 && buildPreview.desc.lodMeshes[0] != null)
                {
                    buildPreview.needModel = true;
                }
                else
                {
                    buildPreview.needModel = false;
                }
                buildPreview.isConnNode = true;
                bool isInserter = buildPreview.desc.isInserter;

                if (isInserter)
                {
                    Pose objectPose2 = Traverse.Create(_this).Method("GetObjectPose2", buildPreview.objId).GetValue<Pose>();
                    buildPreview.lpos2 = objectPose2.position;
                    buildPreview.lrot2 = objectPose2.rotation;
                }
                if ((buildPreview.lpos - _this.player.position).sqrMagnitude > _this.player.mecha.buildArea * _this.player.mecha.buildArea)
                {
                    buildPreview.condition = EBuildCondition.OutOfReach;
                    //_this.cursorText = "目标超出范围".Translate();
                    _this.cursorWarning = true;
                }
                else
                {
                    buildPreview.condition = EBuildCondition.Ok;
                    //_this.cursorText = "拆除".Translate() + buildPreview.item.name;
                }
                if (buildPreview.desc.multiLevel)
                {
                    bool flag;
                    int num;
                    int num2;
                    factory.ReadObjectConn(buildPreview.objId, 15, out flag, out num, out num2);
                    if (num != 0)
                    {
                        buildPreview.condition = EBuildCondition.Covered;
                        //_this.cursorText = buildPreview.conditionText;
                    }
                }
            }
        }

        /// <summary>
        /// 根据敲击的键盘按键，执行对应的逻辑，只使用了方向键的上下左右
        /// ← 减速
        /// →加速
        /// ↑倒放
        /// ↓正放
        /// </summary>
        [HarmonyPrefix, HarmonyPatch(typeof(PlayerAction_Build), "DetermineDestructPreviews")]
        public static bool DetermineDestructPreviewsPatch(PlayerAction_Build __instance)
        {
            var _this = __instance;
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                VideoChars.SpeedDown();
                _this.cursorText = VideoChars.GetSpeedConfig().title;
                return true;
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                VideoChars.SpeedUp();
                _this.cursorText = VideoChars.GetSpeedConfig().title;
                return true;
            }
            if (!Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow)) return true;

            if (Input.GetKey(KeyCode.UpArrow))
            {
                VideoChars.isOpenInversion = true;
                _this.cursorText = VideoChars.GetSpeedConfig().title + "倒放";

            } else if (Input.GetKey(KeyCode.DownArrow))
            {
                VideoChars.isOpenInversion = false;
                _this.cursorText = VideoChars.GetSpeedConfig().title;
            }
            

            if (loadBeltList.Count() != 0)
            {
                _this.ClearBuildPreviews();
                List<int> removeList = VideoChars.GetCurrentFrameData();
                List<EntityData> beltList = new List<EntityData>();
                for (int i = 0; i < loadBeltList.Count; i++)
                {
                    beltList.Add(loadBeltList[i]);
                }
                for (int i = removeList.Count - 1; i >= 0; i--)
                {
                    //Debug.Log(i);
                    beltList.RemoveAt(removeList[i]);
                }

                CustomDestructPreviews(__instance, beltList);
                VideoChars.PlayNextFrame();
                return false;
            } else
            {

                /*if (!VFInput.onGUI)
                {
                    UICursor.SetCursor(ECursor.Delete);
                }*/
                _this.previewPose.position = Vector3.zero;
                _this.previewPose.rotation = Quaternion.identity;
                PlanetFactory factory = Traverse.Create(_this).Field("factory").GetValue<PlanetFactory>();

                if (_this.castObjId != 0)
                {
                    ItemProto itemProto = Traverse.Create(_this).Method("GetItemProto", _this.castObjId).GetValue<ItemProto>();
                    if (itemProto != null)
                    {
                        _this.ClearBuildPreviews();
                        List<EntityData> beltList;
                        if (factory.entityPool[_this.castObjId].beltId != 0)
                        {
                            var path = GetPathByBeltId(factory, factory.entityPool[_this.castObjId].beltId);
                            beltList = GetBletsByPath(factory, path);
                        }
                        else
                        {
                            beltList = GetEntitysByProto(factory, itemProto);
                        }
                        loadBeltList = beltList;
                        CustomDestructPreviews(__instance, beltList);
                    }
                    else
                    {
                        _this.ClearBuildPreviews();
                    }
                }
                else
                {
                    _this.ClearBuildPreviews();
                }
                return false;
            }
        }
    }
}
