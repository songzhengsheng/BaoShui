using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using OracleBase.Models;

namespace OracleBase.HelpClass
{
    public class CommonEFhelp
    {
        readonly Entities dbContext = new Entities();
        public bool add(object model)
        {       
            try
            {
               dbContext.Entry<object>(model).State = EntityState.Added;
                int c = dbContext.SaveChanges();
                if (c > 0)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
           
           
        }
        public bool del(object model)
        {
            try
            {
                dbContext.Entry<object>(model).State = EntityState.Deleted;
                int c = dbContext.SaveChanges();
                if (c > 0)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }


        }

        public bool Edit(object model)
        {
            try
            {
                dbContext.Entry<object>(model).State = EntityState.Modified;
                int c = dbContext.SaveChanges();
                if (c > 0)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }


        }

    }
}