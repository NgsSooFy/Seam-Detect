//----------------------------------------------------------------------------- 
// <copyright file="DataExporter.cs" company="KEYENCE">
//	 Copyright (c) 2013 KEYENCE CORPORATION.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------- 
using System;
using System.IO;
using System.Text;
using QCDS_TestDemo.Datas;

namespace QCDS_TestDemo
{
	/// <summary>
	/// Data export class
	/// </summary>
	public static class DataExporter
	{
		#region Method

		/// <summary>
		/// Profile output
		/// </summary>
		/// <param name="datas">Profile data</param>
		/// <param name="profileNo">Profile information</param>
		/// <param name="fileName">File name</param>
		/// <returns></returns>
		static public bool ExportOneProfile(ProfileData[] datas, int profileNo, string fileName)
		{
			try
			{
				Encoding unicode = System.Text.Encoding.GetEncoding("utf-16");
				using (StreamWriter sw = new StreamWriter(fileName, false, unicode))
				{
					try
					{
						if (datas[0] == null) return false;
						sw.WriteLine(datas[profileNo].ToString());
					}
					finally
					{
						sw.Close();
					}
				}
			}
			catch (Exception ex)
			{
				// File save failure
				System.Diagnostics.Debug.WriteLine(ex.Message);
				System.Diagnostics.Debug.Assert(false);
				return false;
			}

			return true;
		}
		
		/// <summary>
		/// Measurement value output
		/// </summary>
		/// <param name="datas">Measurement data</param>
		/// <param name="fileName">File name</param>
		/// <returns></returns>
		static public bool ExportMeasureData(MeasureData[] datas, string fileName)
		{
			try
			{
				Encoding unicode = System.Text.Encoding.GetEncoding("utf-16");

				using (StreamWriter sw = new StreamWriter(fileName, false, unicode))
				{
					// File output to all receivers
					for (int i = 0; i < datas.Length; i++)
					{
						sw.WriteLine(datas[i].ToString());
					}
				}
			}
			catch (Exception ex)
			{
				// File save failure
				System.Diagnostics.Debug.WriteLine(ex.Message);
				System.Diagnostics.Debug.Assert(false);
				return false;
			}

			return true;
		}

		#endregion
	}
}
