﻿using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BimBuildings.Util
{
    public class FailureHandler : IFailuresPreprocessor
    {
        public string ErrorMessage { set; get; }
        public string ErrorSeverity { set; get; }

        public FailureHandler()
        {
            ErrorMessage = "";
            ErrorSeverity = "";
        }

        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            IList<FailureMessageAccessor> failureMessages = failuresAccessor.GetFailureMessages();

            foreach (FailureMessageAccessor failureMessageAccessor in failureMessages)
            {
                // We're just deleting all of the warning level 
                // failures and rolling back any others

                FailureDefinitionId id = failureMessageAccessor.GetFailureDefinitionId();

                try
                {
                    ErrorMessage = failureMessageAccessor.GetDescriptionText();
                }
                catch
                {
                    ErrorMessage = "Unknown Error";
                }

                try
                {
                    FailureSeverity failureSeverity = failureMessageAccessor.GetSeverity();

                    ErrorSeverity = failureSeverity.ToString();

                    if (failureSeverity == FailureSeverity.Warning)
                    {
                        failuresAccessor.DeleteWarning(failureMessageAccessor);
                    }
                    else
                    {
                        return FailureProcessingResult.ProceedWithRollBack;
                    }
                }
                catch
                {
                }
            }
            return FailureProcessingResult.Continue;
        }
    }
}
