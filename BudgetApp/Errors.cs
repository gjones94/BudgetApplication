using BudgetApp.ViewModels.ErrorViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Identity.Client;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BudgetApp
{
    public class Errors
    {
        public enum ErrorType
        {
            NotFound,
            Unauthorized,
            Unauthenticated,
            InvalidOperation,
            OperationFailed,
            InvalidToken,
            ExpiredToken
        }
      
        public class Default
        {
            public const string Title = "Error";
            public const string Message = "An error occurred while processing your request.";
            public const string Action = "Please notify your administrator of error details for assistance";
        }

        public class NotFound
        {
            public const string Title = "Not Found";
            public const string Message = "The requested resource could not be located";
            public const string Action = "The resource may have been relocated or removed";
        }

        public class Unauthorized
        {
            public const string Title = "Unauthorized";
            public const string Message = "You are not authorized to perform this action";
            public const string Action = "Please contact your administrator if you require access";
        }

        public class Unauthenticated
        {
            public const string Title = "Unauthenticated";
            public const string Message = "You are not authenticated";
            public const string Action = "Please register or login";
        }

        public class InvalidOperation
        {
            public const string Title = "Operation Not Permitted";
            public const string Message = "The operation you requested is not allowed by the system";
            public const string Action = "Please contact your administrator for additional information";
        }

        public class OperationFailed
        {
            public const string Title = "Operation Failed";
            public const string Message = "The operation attempted failed";
            public const string Action = "Please try again. If the problem persists, please contact your administrator";
        }

        public class InvalidToken
        {
            public const string Title = "Invalid Token";
            public const string Message = "The token submitted is not a valid token";
            public const string Action = OperationFailed.Action;
        }
        public class ExpiredToken
        {
            public const string Title = InvalidToken.Title;
            public const string Message = "The invite you are trying to use has expired";
            public const string Action = "Please ask the individual who invited you to send another invite";
        }

        public static ErrorViewModel GetErrorViewModel(ErrorType errorType, string? title = null, string? message = null, string? action = null)
        {
            //set up default error messages
            string defaultTitle = Default.Title;
            string defaultMessage = Default.Message;
            string defaultAction = Default.Action;

            switch (errorType)
            {
                case ErrorType.Unauthenticated:
                    defaultTitle = Unauthenticated.Title;
                    defaultMessage = Unauthenticated.Message;
                    defaultAction = Unauthenticated.Action;
                    break;
                case ErrorType.Unauthorized:
                    defaultTitle = Unauthorized.Title;
                    defaultMessage = Unauthorized.Message;
                    defaultAction = Unauthorized.Action;
                    break;
                case ErrorType.NotFound:
                    defaultTitle = NotFound.Title;
                    defaultMessage = NotFound.Message;
                    defaultAction = NotFound.Action;
                    break;
                case ErrorType.InvalidOperation:
                    defaultTitle = InvalidOperation.Title;
                    defaultMessage = InvalidOperation.Message;
                    defaultAction = InvalidOperation.Action;
                    break;
                case ErrorType.OperationFailed:
                    defaultTitle = OperationFailed.Title;
                    defaultMessage = OperationFailed.Message;
                    defaultAction = OperationFailed.Action;
                    break;
                case ErrorType.InvalidToken:
                    defaultTitle = InvalidToken.Title;
                    defaultMessage = InvalidToken.Message;
                    defaultAction = InvalidToken.Action;
                    break;
                case ErrorType.ExpiredToken:
                    defaultTitle = ExpiredToken.Title;
                    defaultMessage = ExpiredToken.Message;
                    defaultAction = ExpiredToken.Action;
                    break;
            }

            ErrorViewModel viewModel = new ErrorViewModel()
            {
                Title = title ?? defaultTitle,
                Message = message ?? defaultMessage,
                Action = action ?? defaultAction,
            };

            return viewModel;
        }

    }
}
