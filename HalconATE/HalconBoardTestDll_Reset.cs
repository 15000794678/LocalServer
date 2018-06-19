using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HalconDotNet;
using System.IO;
using IdentifScreem_Reset;

namespace HalconBoardTestDll_Reset
{
    class HalconBoardTestDll
    {
        private readonly HDevelopExport _halcon = new HDevelopExport();
        private const int Empty = 0;
        private HTuple _acqHandle = new HTuple(0);
        private HObject _screemImage = new HObject();
        private HObject _cammeraImage= new HObject();

        #region 内部函数
        private bool HasConnectedToCamera()   //what mean?
        {
            return _acqHandle.TupleEqual(Empty) <= 0;
        }

        private bool ConnectToCamera()       //what mean?
        {
            if (!HasConnectedToCamera())
            {
                _halcon.ConnectCamera(out _acqHandle);
                return HasConnectedToCamera();
            }
            return false;
        }

        private bool DisconnectFromCamera()
        {
            if (HasConnectedToCamera())
            {
                _halcon.DisconnectCamera(_acqHandle);
                _acqHandle = Empty;
            }
            return true;
        }

        private bool GrabImage()
        {
            if (!HasConnectedToCamera() && !ConnectToCamera())
            {
                return false;
            }
            try
            {
                _halcon.get_image_from_camera(ref _cammeraImage, _acqHandle);
                return true;
            }
            catch (HOperatorException e)
            {
                throw new Exception(e.Message);
                //return false;
            }
        }

        private bool ReadLightState(int blueValue,int redValue,int yellowValue,out List<int> lightState )
        {
            lightState = null;
            try
            {
                HTuple hvResult;
                _halcon.read_light_state(_cammeraImage,blueValue,redValue,yellowValue,out hvResult);
                lightState = InsertTupleToList(hvResult);
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        private bool ImageFromFile(string imagePath)
        {
            try
            {
                _halcon.get_image_from_file(ref _screemImage, imagePath);
                return true;
            }
            catch
            {
                //_imageLog.WriteLogFile("没有读取到图片");
                return false;
                //throw new Exception(e.Message);
            }
        }

        private bool DoInspect(string lightModelPath,string resetModelPath,string cancelModelPath,out List<int> lightResult)
        {
            lightResult = null;
            try
            {
                HTuple hvResult;
                _halcon.do_inspect(_screemImage, lightModelPath, resetModelPath, cancelModelPath, out hvResult);
                lightResult = InsertTupleToList(hvResult);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool DoInspect1(string okPath, string notokPath, string testingPath, string startingPath, out List<int> testResult)
        {
            testResult = null;
            try
            {
                HTuple hvResult;
                _halcon.do_inspect1(_screemImage, okPath, notokPath, testingPath, startingPath, out hvResult);
                testResult = InsertTupleToList(hvResult);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            
        }

        private bool DoInspect3(string testNumPath,string testTimePath,out List<int> testResult)
        {
            testResult = null;
            try
            {
                HTuple hvResult;
                _halcon.do_inspect3(_screemImage, testNumPath, testTimePath, out hvResult);
                testResult = InsertTupleToList(hvResult);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private List<int> InsertTupleToList(HTuple tuples)
        {
            List<int> lstResult = new List<int>();
            for (int i = 0; i < tuples.Length; i++)
            {
                lstResult.Add(tuples[i].I);
            }
            return lstResult;
        }
        #endregion


        public int Halcon_CheckModule(string boxModelPath, string okPath, string notokPath, string testingPath, string startingPath)
        {
            int fileExist = 0;
            
            if (!HasConnectedToCamera() && !ConnectToCamera())
            {
                return 20;
            }

            return fileExist;
        }

        //程序要改
        public int[] Halcon_CheckImageStatus(string imagePath,string testNumModelPath,string testTimeModelPath ,string lightModelPath,string resetModelPath,string cancelModelPath ,string okPath, string notokPath, string testingPath, string startingPath)
        {
            int[] outResult=new int[3];
            if (ImageFromFile(imagePath))
            {
                List<int> testResultList = null;
                if (DoInspect3(testNumModelPath,testTimeModelPath,out testResultList))
                {
                    if (testResultList[0] == 0)
                    {
                        outResult[0] = 3;
                        outResult[1] = 3;
                        outResult[2] = 3;
                    }
                    else
                    {
                        List<int> outBoxResultList = null;
                        if (DoInspect(lightModelPath, resetModelPath, cancelModelPath, out outBoxResultList))
                        {
                            if (outBoxResultList[0] == 2)
                            {
                                //出现box //两个坐标合并成十六进制
                                outResult[0] = 6;
                                outResult[1] = outBoxResultList[1] << 16;
                                outResult[1]+= outBoxResultList[3];
                                outResult[2] = outBoxResultList[2] << 16;
                                outResult[2]+= outBoxResultList[4];
                            }
                            else if (outBoxResultList[0] == 3)
                            {
                                outResult[0] = 6;
                                outResult[1] = 0;
                                outResult[2] = 0;
                            }
                            else if (outBoxResultList[0] == 1)
                            {
                                outResult[0] = 7;
                                outResult[1] = 0;
                                outResult[2] = 0;
                            }
                            else
                            {
                                List<int> outTestResultList = null;
                                if (DoInspect1(okPath, notokPath, testingPath, startingPath, out outTestResultList))
                                {
                                    if (outTestResultList[0] == 10)
                                    {
                                        //未找到定位区域
                                        outResult[0] = 3;
                                        outResult[1] = 3;
                                        outResult[2] = 3;
                                    }
                                    else
                                    {
                                        outResult[0] = 5;
                                        //up测试结果
                                        outResult[1] = outTestResultList[0];
                                        outResult[2] = 0;

                                    }

                                }
                                else
                                {
                                    outResult[0] = 4;
                                    outResult[1] = 4;
                                    outResult[2] = 4;
                                }
                            }
                        }
                        else
                        {
                            outResult[0] = 2;
                            outResult[1] = 2;
                            outResult[2] = 2;  
                        }
                        
                    }
                }
                else
                {
                    outResult[0] = 2;
                    outResult[1] = 2;
                    outResult[2] = 2; 
                }
            }
            else
            {
                //未读到图
                outResult[0] = 0;
                outResult[1] = 0;
                outResult[2] = 0;
            }

            return outResult;
        }

        public int Halcon_CheckLightStatus(int blueValue,int redValue,int yellowValue, out int colorValue)
        {
            colorValue = 0;
            if (!HasConnectedToCamera() && !ConnectToCamera())
            {

                return 20; //连接不到相机
            }
            try
            {
                if (!GrabImage())
                {
                    return 21; //无图片
                }
            }
            catch (HOperatorException)
            {
                if (DisconnectFromCamera() && ConnectToCamera())
                {
                    try
                    {
                        if (!GrabImage())
                        {
                            return 21; //无图片
                        }
                    }
                    catch (HOperatorException e)
                    {
                        throw new Exception(e.Message);
                    }
                }
            }

            List<int> lightState = null;

            if (ReadLightState(blueValue,redValue,yellowValue,out lightState))
            {
                colorValue = lightState[1];
                //  0 没有灯亮，1黄灯，2红灯，3蓝灯
                return lightState[0];
            }

            return 22; //颜色不在范围内
        }

    }
}
