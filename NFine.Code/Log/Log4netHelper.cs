using System;
using System.IO;
using System.Reflection;
using System.Web;

namespace NFine.Code
{
    public class Log4NetHelper
    {
        private readonly log4net.ILog _log;
        public Log4NetHelper()
        {
            FileInfo configFile = new FileInfo(HttpContext.Current.Server.MapPath("/Configs/log4net.config"));
            log4net.Config.XmlConfigurator.Configure(configFile);
            //log4net.Config.XmlConfigurator.Configure();
            _log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }
        /// <summary>
        /// 程序运行的过程中的，一般信息可以调用此方法记录日记
        /// </summary>
        /// <param name="info"></param>
        public void Info(string info)
        {
            if (_log.IsInfoEnabled)
            {
                _log.Info(info);
            }
        }
        /// <summary>
        /// 程序运行的过程中的，一般信息可以调用此方法记录日记
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ex"></param>
        public void Info(string info, Exception ex)
        {
            if (_log.IsInfoEnabled)
            {
                _log.Info(info, ex);
            }
        }
        /// <summary>
        /// 程序出现错误的时候调用此方法记录日记（一般用在出现了异常以后）
        /// </summary>
        /// <param name="info"></param>
        public void Error(string info)
        {
            if (_log.IsErrorEnabled)
            {
                _log.Error(info);
            }
        }
        /// <summary>
        ///  程序出现错误的时候调用此方法记录日记（一般用在出现了异常以后）
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ex"></param>
        public void Error(string info, Exception ex)
        {
            if (_log.IsErrorEnabled)
            {
                _log.Error(info, ex);
            }
        }
        /// <summary>
        /// 程序员觉得任何有利于程序在调试时更详细的了解系统运行状态的信息，比如变量的值等等，都可以调用此方法记录到日记
        /// </summary>
        /// <param name="info"></param>
        public void Debug(string info)
        {
            if (_log.IsDebugEnabled)
            {
                _log.Debug(info);
            }
        }
        /// <summary>
        /// 程序员觉得任何有利于程序在调试时更详细的了解系统运行状态的信息，比如变量的值等等，都可以调用此方法记录到日记
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ex"></param>
        public void Debug(string info, Exception ex)
        {
            if (_log.IsDebugEnabled)
            {
                _log.Debug(info, ex);
            }
        }
        /// <summary>
        /// 程序出现警告时调用此方法记录日记（程序出现警告不会使程序出现异常，但是可能会影响程序性能）
        /// </summary>
        /// <param name="info"></param>
        public void Warn(string info)
        {
            if (_log.IsWarnEnabled)
            {
                _log.Warn(info);
            }
        }
        /// <summary>
        ///  程序出现警告时调用此方法记录日记（程序出现警告不会使程序出现异常，但是可能会影响程序性能）
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ex"></param>
        public void Warn(string info, Exception ex)
        {
            if (_log.IsWarnEnabled)
            {
                _log.Warn(info, ex);
            }
        }
        /// <summary>
        /// 程序出现特别严重的错误，一般是在应用程序崩溃的时候调用此方法记录日记
        /// </summary>
        /// <param name="info"></param>
        public void Fatal(string info)
        {
            if (_log.IsFatalEnabled)
            {
                _log.Fatal(info);
            }
        }
        /// <summary>
        /// 程序出现特别严重的错误，一般是在应用程序崩溃的时候调用此方法记录日记
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ex"></param>
        public void Fatal(string info, Exception ex)
        {
            if (_log.IsFatalEnabled)
            {
                _log.Fatal(info, ex);
            }
        }
    }
}