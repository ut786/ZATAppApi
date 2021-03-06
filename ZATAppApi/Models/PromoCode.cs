﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Runtime.Serialization;
using ZATAppApi.Models.Exceptions;

namespace ZATAppApi.Models
{
    /// <summary>
    /// Promo Codes are the discount coupons to be added with rides
    /// </summary>
    public class PromoCode : DbModel
    {
        private int id;
        private string code;
        private short discountPercent;
        private bool isOpen;
        /// <summary>
        /// Constructor to initialize values from database
        /// </summary>
        /// <param name="id"></param>
        public PromoCode(int id)
        {
            dbCommand = new SqlCommand("GetPromoCode", dbConnection);
            dbCommand.CommandType = System.Data.CommandType.StoredProcedure;
            dbCommand.Parameters.Add(new SqlParameter("@pId", System.Data.SqlDbType.Int)).Value = id;
            dbConnection.Open();
            try
            {
                using (dbReader = dbCommand.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        this.id = id;
                        code = (string)dbReader[1];
                        discountPercent = (short)dbReader[2];
                        isOpen = (bool)dbReader[3];
                    }
                }
            }
            catch (SqlException ex)
            {
                dbConnection.Close();
                throw new DbQueryProcessingFailedException("PromoCode->Constructor(int)", ex);
            }
            dbConnection.Close();
        }
        /// <summary>
        /// Constructor to add new Data for promo code
        /// </summary>
        /// <param name="code">Unique Code to represent the promotion</param>
        /// <param name="discountPercent">Percentage of discount to be added to the ride total fare</param>
        public PromoCode(string code, short discountPercent)
        {
            if (code.Length > 20)
            {
                throw new ValueLengthExceedsException(code, 20);
            }
            dbCommand = new SqlCommand("AddNewPromoCode", dbConnection);
            dbCommand.CommandType = System.Data.CommandType.StoredProcedure;
            dbCommand.Parameters.Add(new SqlParameter("@code", System.Data.SqlDbType.VarChar)).Value = code;
            dbCommand.Parameters.Add(new SqlParameter("@discount", System.Data.SqlDbType.SmallInt)).Value = discountPercent;
            dbConnection.Open();
            try
            {
                id = Convert.ToInt32(dbCommand.ExecuteScalar());
            }
            catch (SqlException ex)
            {
                dbConnection.Close();
                if (ex.Number == 2627)
                {
                    throw new UniqueKeyViolationException("Promo Code already present.Same Promo Code cannot be added twice.");
                }
                throw new DbQueryProcessingFailedException("PromoCode->Constructor(string,short)", ex);
            }
            dbConnection.Close();
            this.code = code;
            this.discountPercent = discountPercent;
        }
        /// <summary>
        /// Percentage of discount to be added to the total fare
        /// </summary>
        [DataMember]
        public short DiscountPercent
        {
            get { return discountPercent; }
        }
        /// <summary>
        /// Unique code to identify the Promotion
        /// </summary>
        [DataMember]
        public string Code
        {
            get { return code; }
        }
        /// <summary>
        /// Primary key
        /// </summary>
        [DataMember]
        public int PromoId
        {
            get { return id; }
        }
        /// <summary>
        /// Property which shows that the promo code is open for application
        /// </summary>
        [DataMember]
        public bool IsOpen
        {
            get
            {
                return isOpen;
            }
            set
            {
                dbCommand = new SqlCommand("UpdateIsOpenPromo", dbConnection);
                dbCommand.CommandType = System.Data.CommandType.StoredProcedure;
                dbCommand.Parameters.Add(new SqlParameter("@pId", System.Data.SqlDbType.Int)).Value = id;
                dbCommand.Parameters.Add(new SqlParameter("@isOpen", System.Data.SqlDbType.Bit)).Value = value;
                dbConnection.Open();
                try
                {
                    dbCommand.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    dbConnection.Close();
                    throw new DbQueryProcessingFailedException("PromoCode->IsOpen", ex);
                }
                dbConnection.Close();
                isOpen = value;
            }
        }
        /// <summary>
        /// Method which will return true if the rider has already used a Promo Code
        /// </summary>
        /// <param name="rider">User who uses the promo code</param>
        /// <returns></returns>
        public bool IsUsed(Rider rider)
        {
            bool flag = false;
            dbCommand = new SqlCommand("IsPromoCodeUsed", dbConnection);
            dbCommand.CommandType = System.Data.CommandType.StoredProcedure;
            dbCommand.Parameters.Add(new SqlParameter("@pId", System.Data.SqlDbType.Int)).Value = id;
            dbCommand.Parameters.Add(new SqlParameter("@uId", System.Data.SqlDbType.BigInt)).Value = rider.UserId;
            dbConnection.Open();
            try
            {
                flag = Convert.ToBoolean(dbCommand.ExecuteScalar());
            }
            catch (SqlException ex)
            {
                dbConnection.Close();
                throw new DbQueryProcessingFailedException("PromoCode->IsUsed", ex);
            }
            dbConnection.Close();
            return flag;
        }
        /// <summary>
        /// Method to get Promo Code by entering the code
        /// </summary>
        /// <param name="code">String of unique words represent as a code</param>
        /// <returns></returns>
        public static PromoCode GetPromoCode(string code)
        {
            int id;
            SqlConnection dbConnection = new SqlConnection(CONNECTION_STRING);
            SqlCommand dbCommand = new SqlCommand("GetPromoCodeByCode", dbConnection);
            dbCommand.CommandType = System.Data.CommandType.StoredProcedure;
            dbCommand.Parameters.Add(new SqlParameter("@code", System.Data.SqlDbType.VarChar)).Value = code;
            dbConnection.Open();
            try
            {
                id = Convert.ToInt16(dbCommand.ExecuteScalar());
            }
            catch (SqlException ex)
            {
                dbConnection.Close();
                throw new DbQueryProcessingFailedException("PromoCode->GetPromoCode", ex);
            }
            dbConnection.Close();
            return new PromoCode(id);
        }
        /// <summary>
        /// Method to get total discounts given in a certain month
        /// </summary>
        /// <param name="month">Month for which the data is collected</param>
        /// <returns></returns>
        public static decimal GetTotalDiscountsByMonth(DateTime month)
        {
            decimal total = 0;
            DateTime startDate = new DateTime(month.Year, month.Month, 1), endDate = startDate.AddMonths(1).AddDays(-1);
            SqlConnection dbConnection = new SqlConnection(CONNECTION_STRING);
            SqlCommand dbCommand = new SqlCommand("GetMonthDiscounts", dbConnection);
            dbCommand.CommandType = System.Data.CommandType.StoredProcedure;
            dbCommand.Parameters.Add(new SqlParameter("@startDate", System.Data.SqlDbType.DateTime)).Value = startDate;
            dbCommand.Parameters.Add(new SqlParameter("@endDate", System.Data.SqlDbType.DateTime)).Value = endDate;
            dbConnection.Open();
            try
            {
                total = Convert.ToDecimal(dbCommand.ExecuteScalar());
            }
            catch (SqlException ex)
            {
                dbConnection.Close();
                throw new DbQueryProcessingFailedException("PromoCode->GetTotalDiscountsByMonth", ex);
            }
            dbConnection.Close();
            return decimal.Round(total);
        }
        /// <summary>
        /// Get list of all promo codes present in the database
        /// </summary>
        /// <returns>List of Promo Codes in Chronological Order</returns>
        public static List<PromoCode> GetAllPromoCodes()
        {
            List<PromoCode> lstPromoCodes = new List<PromoCode>();
            SqlConnection dbConnection = new SqlConnection(CONNECTION_STRING);
            SqlCommand dbCommand = new SqlCommand("SELECT [PromoId] FROM [dbo].[PROMO_CODES] ORDER BY [PromoId] DESC", dbConnection);
            dbConnection.Open();
            try
            {
                using (SqlDataReader dbReader = dbCommand.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        lstPromoCodes.Add(new PromoCode((int)dbReader[0]));
                    }
                }
            }
            catch (SqlException ex)
            {
                dbConnection.Close();
                throw new DbQueryProcessingFailedException("PromoCode->GetAllPromoCodes", ex);
            }
            dbConnection.Close();
            return lstPromoCodes;
        }
    }
}