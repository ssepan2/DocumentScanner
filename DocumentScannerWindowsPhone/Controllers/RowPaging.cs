using System;
//using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using System.Web;

namespace Ssepan.Data.UI
{
    public class RowPaging
    {
        #region Declarations
        private const Int32 FirstPageDefault = 1;
        private const Int32 RowsPerPageDefault = 10;
        private const Int32 CurrentPageDefault = FirstPageDefault;
        #endregion Declarations

        #region Constructors
        public RowPaging
        (
            Int32 rowCount,
            Int32 rowsPerPage = RowsPerPageDefault,
            Int32 currentPage = CurrentPageDefault 
        )
        {
            RowCount = rowCount;
            RowsPerPage = rowsPerPage;
            CurrentPage = currentPage;
        }
        #endregion Constructors

        #region Properties
        private Int32 _RowCount = default(Int32);
        public Int32 RowCount
        {
            get { return _RowCount; }
            set { _RowCount = value; }
        }

        private Int32 _RowsPerPage = default(Int32);
        public Int32 RowsPerPage
        {
            get { return _RowsPerPage; }
            set { _RowsPerPage = value; }
        }

        public Int32 FirstPage
        {
            get { return FirstPageDefault; }
        }

        public Int32 LastPage
        {
            get 
            { 
                return (Int32)Math.Ceiling((Double)RowCount / (Double)RowsPerPage);
            }
        }

        private Int32 _CurrentPage = default(Int32);
        public Int32 CurrentPage
        {
            get { return _CurrentPage; }
            set 
            {
                if (value < 1)
                {
                    _CurrentPage = 1;
                }
                else if (value > LastPage)
                {
                    _CurrentPage = LastPage;
                }
                else
                {
                    _CurrentPage = value;
                }
            }
        }

        public Int32 PreviousPage
        {
            get 
            { 
                if (CurrentPage > FirstPage)
                {
                    return CurrentPage - 1;
                }
                else
                {
                    return FirstPage; 
                }
            }
        }

        public Int32 NextPage
        {
            get 
            {
                if (CurrentPage < LastPage)
                {
                    return CurrentPage + 1;
                }
                else
                {
                    return LastPage; 
                }
            }
        }
        #endregion Properties

        #region Methods
        /// <summary>
        /// Given a source list and the defined paging properties, return a page list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public IEnumerable<T> GetPageInList<T>(IEnumerable<T> list)
        {
            IEnumerable<T> returnValue = default(IEnumerable<T>);
            returnValue =  list.Skip((CurrentPage - 1) * RowsPerPage).Take(RowsPerPage).ToList();
            return returnValue;
        }

        public static Int32 NextPageNumberWrapped(Int32 pageNumber, Int32 pageCount)
        {
            Int32 returnValue = default(Int32);

            returnValue = pageNumber;

            //TODO:(Optimization) the next two lines cancel each other out, and can be disabled
            returnValue--;  //convert page number to index
            returnValue++;  //increment offset
            returnValue %= pageCount; //perform modulus
            returnValue++;  //covert index to page number

            return returnValue;
        }
        #endregion Methods

    }
}