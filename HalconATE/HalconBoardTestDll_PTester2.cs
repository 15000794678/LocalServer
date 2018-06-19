using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HalconDotNet;
using System.IO;
using IdentifScreem_PTester2;

namespace HalconBoardTestDll_PTester2
{
    class HalconBoardTestDll
    {
        private readonly HDevelopExport _halcon = new HDevelopExport();
        private HObject _screemImage = new HObject();

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

        private bool DoInspect(string boxModelPath,out List<int> boxResult)
        {
            boxResult = null;
            try
            {
                HTuple hvResult;
                _halcon.do_inspect(_screemImage, boxModelPath,out hvResult);
                boxResult = InsertTupleToList(hvResult);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool DoInspect1(string countPath, string startPath, string pausePath, string stopPath,  out List<int> testResult)
        {
            testResult = null;
            try
            {
                HTuple hvResult;
                _halcon.do_inspect1(_screemImage, countPath, startPath, pausePath,  stopPath,  out hvResult);
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

        public int Halcon_CheckModule(string boxModelPath, string passPath, string failPath, string pausePath, string startPath, string stopPath,string testPath)
        {
            int fileExist = 0;
            if (!File.Exists(boxModelPath))
            {
                fileExist = 1;
            }
            if (!File.Exists(failPath ))
            {
                fileExist = fileExist + 10;
            }
            if (!File.Exists(passPath))
            {
                fileExist = fileExist + 100;
            }
            if (!File.Exists(pausePath))
            {
                fileExist = fileExist + 1000;
            }
            if (!File.Exists(startPath))
            {
                fileExist = fileExist + 10000;
            }
            if (!File.Exists(stopPath))
            {
                fileExist = fileExist + 100000;
            }
            if (!File.Exists(testPath))
            {
                fileExist = fileExist + 1000000;
            }

            return fileExist;
        }

        public int[] Halcon_CheckImageStatus(string imagePath, string boxModelPath, string countPath, string startPath, string pausePath,  string stopPath)
        {
            int[] outResult=new int[3];
            if (ImageFromFile(imagePath))
            {
                List<int> outBoxResultList = null;
                if (DoInspect(boxModelPath, out outBoxResultList))
                {
                    if (outBoxResultList[0] == 1)
                    {
                        //出现box
                        outResult[0] = 1;
                        outResult[1] = outBoxResultList[1];
                        outResult[2] = outBoxResultList[2];
                    }
                    else
                    {
                        List<int> outTestResultList = null;
                        if (DoInspect1(countPath, startPath, pausePath, stopPath,  out outTestResultList))
                        {
                            if (outTestResultList[0] == 3)
                            {
                                //未找到定位区域
                                outResult[0] = 3;
                                outResult[1] = 3;
                                outResult[2] = 3;
                            }
                            else
                            {
                                outResult[0] = 5;
                                outResult[1] = outTestResultList[1];
                                //up测试结果
                                outResult[2] = outTestResultList[2];
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
                    //box出问题
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

    }
}
