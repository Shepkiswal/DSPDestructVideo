﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DSPDestructVideo
{
    class SpeedConfig
    {
        public string title;
        public int value;

        public SpeedConfig(string title, int value)
        {
            this.title = title;
            this.value = value;
        }
    };

    class VideoChars
    {
        /// <summary>
        /// 所有处理后的帧数据
        /// </summary>
        public static List<List<int>> frameData = new List<List<int>>();

        /// <summary>
        /// 当前帧数据对应下标
        /// </summary>
        public static int dataIndex = 0;

        /// <summary>
        /// 是否开启倒放
        /// </summary>
        public static bool isOpenInversion = false;

        /// <summary>
        /// 播放速度配置
        /// </summary>
        private static List<SpeedConfig> speedConfigList = new List<SpeedConfig> {
            new SpeedConfig(title: "0.25倍速", value: 16),
            new SpeedConfig(title: "0.5倍速", value: 8),
            new SpeedConfig(title: "1倍速", value: 4),
            new SpeedConfig(title: "2倍速", value: 2),
            new SpeedConfig(title: "4倍速", value: 1),
        };

        /// <summary>
        /// 当前速度配置对应下标
        /// </summary>
        private static int speedConfigIndex = 2;

        /// <summary>
        /// 载入帧数据
        /// </summary>
        public static void LoadFrameData()
        {
            foreach (var temString in temArr)
            {
                List<int> dataList = CreateAllDataList(temString);
                frameData.Add(dataList);
            }
        }

        /// <summary>
        /// 处理每帧字符数据，转换为传送带List对应下标
        /// </summary>
        public static List<int> CreateAllDataList(string dataString)
        {
            string[] dataArray = dataString.Split('\n');
            string totalText = "";
            for (int i = 0; i < dataArray.Length; i++)
            {
                if (i % 2 == 0)
                {
                    totalText += dataArray[i];
                } else
                {
                    totalText += ReverseA(dataArray[i]);
                }
            }
            int[] removeArray = { };
            List<int> removeList = new List<int>(removeArray);
            for (int i = 0; i < totalText.Count(); i++)
            { 
                if (totalText[i] != Convert.ToChar("X"))
                {
                    removeList.Add(i);
                }
            }
            return removeList;
        }

        /// <summary>
        /// 字符串倒序
        /// </summary>
        public static string ReverseA(string text)
        {
            char[] cArray = text.ToCharArray();
            string reverse = String.Empty;
            for (int i = cArray.Length - 1; i > -1; i--)
            {
                reverse += cArray[i];
            }
            return reverse;
        }

        /// <summary>
        /// 获取当前帧数据
        /// </summary>
        public static List<int> GetCurrentFrameData()
        {
            return frameData[dataIndex];
        }

        /// <summary>
        /// 每帧间隔计数
        /// </summary>
        public static int interval = 0;

        /// <summary>
        /// 播放下一帧，修改List对应下标
        /// </summary>
        public static void PlayNextFrame()
        {
            SpeedConfig speedConfig = GetSpeedConfig();
            if (interval > speedConfig.value)
            {
                interval = 0;
                if (isOpenInversion)
                {
                    if (dataIndex > 0)
                    {
                        dataIndex -= 1;
                    }
                } else
                {
                    if (dataIndex < frameData.Count - 1)
                    {
                        dataIndex += 1;
                    }
                }
            }
            else
            {
                interval += 1;
            }
        }

        /// <summary>
        /// 获取当前速度配置
        /// </summary>
        public static SpeedConfig GetSpeedConfig()
        {
            return speedConfigList[speedConfigIndex];
        }

        /// <summary>
        /// 记录当前时间毫秒值，下次切换时间大于500毫秒才可以切换成功
        /// </summary>
        private static int millisecond = 0;

        /// <summary>
        /// 加速
        /// </summary>
        public static void SpeedUp()
        {
            int nowMillisecond = DateTime.Now.Millisecond;
            if (System.Math.Abs(nowMillisecond - millisecond) > 500)
            {
                if (speedConfigIndex < speedConfigList.Count - 1)
                {
                    speedConfigIndex += 1;
                }
                millisecond = nowMillisecond;
            }
        }

        /// <summary>
        /// 减速
        /// </summary>
        public static void SpeedDown()
        {
            int nowMillisecond = DateTime.Now.Millisecond;
            if (System.Math.Abs(nowMillisecond - millisecond) > 500)
            {
                if (speedConfigIndex > 0)
                {
                    speedConfigIndex -= 1;
                }
                millisecond = nowMillisecond;
            }
        }
    }
}