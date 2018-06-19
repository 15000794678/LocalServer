using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HalconATE
{
    public class MacWrite
    {
        private string notokModelPath = @"notok.ncm";
        private string okModelPath = @"ok.ncm";
        private string startingModelPath = @"starting.ncm";
        private string sureboxModelPath = @"surebox.ncm";
        private string testingModelPaht = @"testing.ncm";

        private string dirPath = string.Empty;

        private HalconBoardTestDll_MacWrite.HalconBoardTestDll _testDll = new HalconBoardTestDll_MacWrite.HalconBoardTestDll();
        /*
         * 返回值 ：
         * 000 = 代表三个都齐全；
         * 111 = 代表全没有，
         * 110 = 代表只有box模板；
         * 101 = 代表只有右测试按钮模板；
         * 011 = 代表只有参数完成模板。   
         */
        public int Halcon_CheckModule(string dirpath)
        {
            int res = 0;
            if (!Directory.Exists(dirpath))
            {
                return 111;
            }

            if (!File.Exists(dirpath + sureboxModelPath))
            {
                res = 1;
            }
            if (!File.Exists(dirpath + okModelPath))
            {
                res += 10;
            }

            if (!File.Exists(dirpath + notokModelPath))
            {
                res += 100;
            }

            if (!File.Exists(dirpath + testingModelPaht))
            {
                res += 1000;
            }

            if (!File.Exists(dirpath + startingModelPath))
            {
                res += 10000;
            }

            dirPath = dirpath;
            return res;
        }

        /*
         * 返回值为含三个值得int数组
         * [0,0,0] 未读到图
         * [1,x,y] 有messagebox窗口，x，y为坐标
         * [2,2,2] messagebox窗口未知错误
         * [3,3,3] 未找到定位区域（右测试按钮）
         * [4,4,4] 未知错误
         * [5,a,b] 查询到测试窗体，其中a和b的值只有0,1,2,3,4；
         * 当a=0表示上窗体初始状态，=1测试中，=2测试成功，=3测试失败，=4未知状态
         * 当b=0表示下窗体初始状态，=1测试中，=2测试成功，=3测试失败，=4未知状态
         */
        public int[] Halcon_CheckImageStatus(string imagePath)
        {
            return _testDll.Halcon_CheckImageStatus(imagePath, 
                                                    dirPath + sureboxModelPath, 
                                                    dirPath + okModelPath, 
                                                    dirPath + notokModelPath,
                                                    dirPath + testingModelPaht,
                                                    dirPath + startingModelPath);
        }

    }
}
