using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Navigation;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Controls;
using Ssepan.Data.UI;

namespace DocumentScannerWindowsPhone
{
    /// <summary>
    /// 
    /// Thanks to Mike Clark on 'Cyberherbalist's Blog' at http://cyberherbalist.wordpress.com/category/windows-phone-7-development/
    /// for tip on adding / removing buttons at runtime.
    /// </summary>
    public class ApplicationBarIconButtonPaging
    {

        private RowPaging rowPaging = default(RowPaging);

        //number of buttons that app bar can show (including next-page button)
        public const Int32 ApplicationBarIconButtonPageLimit = 4;
        
        //number of buttons available, excluding next-page button 
        public const Int32 ApplicationBarIconButtonPageEffectiveLimit = ApplicationBarIconButtonPageLimit-1;

        /// <summary>
        /// Initialize application button paging helper.
        /// </summary>
        /// <param name="applicationBarActions">button action definitions (excluding next-page action)</param>
        /// <param name="nextPageAction">next-page action</param>
        /// <param name="pageSize">size of page of actions in ApplicationBarActions list.</param>
        public ApplicationBarIconButtonPaging
        (
            List<ApplicationBarAction> applicationBarActions,
            ApplicationBarAction nextPageAction,
            Int32 pageSize
        )
        {
            ApplicationBarActions = applicationBarActions;
            NextPageAction = nextPageAction;

            rowPaging = new RowPaging(ApplicationBarActions.Count, pageSize);
        }

        private List<ApplicationBarAction> _ApplicationBarActions = default(List<ApplicationBarAction>);
        public List<ApplicationBarAction> ApplicationBarActions
        {
            get { return _ApplicationBarActions; }
            set { _ApplicationBarActions = value; }
        }

        private ApplicationBarAction _NextPageAction = default(ApplicationBarAction);
        public ApplicationBarAction NextPageAction
        {
            get { return _NextPageAction; }
            set { _NextPageAction = value; }
        }

        /// <summary>
        /// Determine whether an instance of a button is present in ApplicationBar
        /// </summary>
        /// <param name="applicationBar"></param>
        /// <param name="applicationBarAction"></param>
        /// <returns></returns>
        public static Boolean ButtonPresent
        (
            ApplicationBar applicationBar, 
            ApplicationBarAction applicationBarAction
        )
        {
            Boolean returnValue = default(Boolean);
            try
            {
                ApplicationBarIconButton button = GetAppBarIconButton(applicationBar, applicationBarAction.Key);

                returnValue = (button != null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        /// <summary>
        /// Add an instance of a button to ApplicationBar.
        /// </summary>
        /// <param name="applicationBar"></param>
        /// <param name="applicationBarAction"></param>
        public static void RemoveButton
        (
            ApplicationBar applicationBar, 
            ApplicationBarAction applicationBarAction
        )
        {
            try
            {

                ApplicationBarIconButton button = GetAppBarIconButton(applicationBar, applicationBarAction.Key);

                if (button != null)
                {
                    //detach click event handler first!
                    button.Click -= applicationBarAction.ActionDelegate;

                    applicationBar.Buttons.Remove(button);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Remove an instance of a button from ApplicationBar
        /// </summary>
        /// <param name="applicationBar"></param>
        /// <param name="applicationBarAction"></param>
        public static void AddButton
        (
            ApplicationBar applicationBar, 
            ApplicationBarAction applicationBarAction
        )
        {
            try
            {
                ApplicationBarIconButton button = GetAppBarIconButton(applicationBar, applicationBarAction.Key);

                if (button == null)
                {
                    button = new ApplicationBarIconButton();

                    button.IconUri = applicationBarAction.IconUri; 
                    button.Text = applicationBarAction.Key; 
                    button.Click += applicationBarAction.ActionDelegate; 

                    applicationBar.Buttons.Add(button);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Calculate next page nubmer and render next page.
        /// </summary>
        /// <param name="applicationBar"></param>
        public void NextPage
        (
            ApplicationBar applicationBar
        )
        {
            Int32 oldPage = default(Int32);
            Int32 newPage = default(Int32);

            //remember old page
            oldPage = rowPaging.CurrentPage;

            //calculate incrementd page, using modulus to wrap to beginning from end
            newPage = RowPaging.NextPageNumberWrapped(rowPaging.CurrentPage, rowPaging.LastPage);

            //render page of button actions, by first unloading old page, then loading new page
            RenderPage(applicationBar, oldPage, newPage);
        }

        
        /// <summary>
        /// Create a page of application bar icon buttons from actions.
        /// </summary>
        /// <param name="applicationBar"></param>
        /// <param name="oldPageNumber"></param>
        /// <param name="newPageNumber"></param>
        public void RenderPage
        (
            ApplicationBar applicationBar,
            Int32 oldPageNumber,
            Int32 newPageNumber
        )
        {
            List<ApplicationBarAction> applicationBarActions = default(List<ApplicationBarAction>);

            if (oldPageNumber == -1)
            { 
                //clear application bar buttons
                applicationBar.Buttons.Clear();
            }
            else
            {
                //get actions for old page number
                applicationBarActions = rowPaging.GetPageInList<ApplicationBarAction>(ApplicationBarActions.AsEnumerable<ApplicationBarAction>()).ToList<ApplicationBarAction>();

                //unload actions for old page number
                foreach (ApplicationBarAction action in applicationBarActions)
                {
                    ApplicationBarIconButtonPaging.RemoveButton(applicationBar, action);
                }

                //unload next-page action
                if ((rowPaging.LastPage > 1) && (NextPageAction != null))
                {
                    ApplicationBarIconButtonPaging.RemoveButton(applicationBar, NextPageAction);
                }
            }


            //change page number
            rowPaging.CurrentPage = newPageNumber;
            

            //get actions for new page number
            applicationBarActions = rowPaging.GetPageInList<ApplicationBarAction>(ApplicationBarActions.AsEnumerable<ApplicationBarAction>()).ToList<ApplicationBarAction>();

            //load actions for new page number
            foreach (ApplicationBarAction action in applicationBarActions)
            {
                ApplicationBarIconButtonPaging.AddButton(applicationBar, action);
            }

            //reload next-page action
            if ((rowPaging.LastPage > 1) && (NextPageAction != null))
            {
                ApplicationBarIconButtonPaging.AddButton(applicationBar, NextPageAction);
            }
        }


        /// <summary>
        /// Get ApplicationBarIconButton by name.
        /// Thanks to James Curran on 'Code'n'Stuff' at http://honestillusion.com/blogs/blog_0/archive/2011/05/24/get-an-applicationbariconbutton-by-name-redux.aspx
        /// for tip on casting non-generic IList to LINQ-compatible list.
        /// </summary>
        /// <param name="applicationBar"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static ApplicationBarIconButton GetAppBarIconButton(ApplicationBar applicationBar, String name)
        {
            ApplicationBarIconButton returnValue = default(ApplicationBarIconButton);
            
            returnValue = applicationBar.Buttons.Cast<ApplicationBarIconButton>().FirstOrDefault(b => b.Text == name);

            //foreach (ApplicationBarIconButton b in applicationBar.Buttons)
            //{
            //    if (b.Text == name)
            //    {
            //        returnValue = b;
            //        break;
            //    }
            //}

            return returnValue;
        }
    }
}
