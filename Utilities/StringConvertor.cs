using System;
using System.Collections.Generic;
using Pegasus_backend.Models;


namespace Pegasus_backend.Utilities
{
    public static class StringConvertor
    {
        public  static Result<List<int>> ToListOfID(this string idStr)
        {
            var result = new Result<List<int>>();
            result.Data = new List<int>();
            string[] idArr;
            List<int> orgIDs = new List<int>();

            if (idStr.Length <= 0)
            {
                idArr = new string[] { };
                result.IsSuccess = false;
                result.ErrorMessage = "id is required";
                return result;
            }
            else if (idStr.Length == 1)
            {
                idArr = new string[] { idStr };
            }
            else
            {
                idArr = idStr.Split(new char[] { ',' });
            }
            for (var i = 0; i < idArr.Length; i++)
            {
                try
                {
                    result.Data.Add(Int32.Parse(idArr[i]));
                }
                catch (Exception ex)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = ex.Message;
                    return result;
                }
            }
            return result;
        }
    }
}
