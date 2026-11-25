using System;
using System.Collections.Generic;
using VoyageHub.Data.Entities;

namespace VoyageHub.Business.Helpers;

/// <summary>
/// Evaluates a travel request against departmental travel policy rules and produces human-readable violation reasons.
/// </summary>
public static class PolicyEvaluator
{
    public static (bool IsFlagged, List<string> Reasons) Evaluate(
        decimal estimatedBudget,
        string? travelClass,
        TravelPolicy? policy,
        string? departmentName = null,
        string? existingStatus = null,
        bool isInternational = false)
    {
        var reasons = new List<string>();
        var deptLabel = string.IsNullOrWhiteSpace(departmentName) ? "Department" : departmentName;

        if (policy != null && policy.HardEnforcement)
        {
            var applicableLimit = isInternational
                ? (policy.IntlBudgetLimit > 0 ? policy.IntlBudgetLimit : policy.MaxBudget)
                : (policy.DomesticLimit > 0 ? policy.DomesticLimit : (policy.IntlBudgetLimit > 0 ? policy.IntlBudgetLimit : policy.MaxBudget));
            var limitLabel = isInternational ? "international budget limit" : "domestic budget limit";

            if (applicableLimit > 0 && estimatedBudget > applicableLimit)
            {
                reasons.Add($"Estimated budget (₹{estimatedBudget:N0}) exceeds {deptLabel} {limitLabel} (₹{applicableLimit:N0}).");
            }

            if (!string.IsNullOrWhiteSpace(policy.AllowedClass) && !string.IsNullOrWhiteSpace(travelClass))
            {
                var allowed = policy.AllowedClass.Trim().ToUpperInvariant();
                var requested = travelClass.Trim().ToUpperInvariant();

                var isBusinessOrFirst = requested.Contains("BUSINESS") || requested.Contains("FIRST");
                if ((allowed == "ECONOMY" || allowed == "ECONOMY+") && isBusinessOrFirst)
                {
                    reasons.Add($"Requested travel class ({travelClass}) exceeds {deptLabel} allowed class ({policy.AllowedClass}).");
                }
            }
        }

        if (reasons.Count == 0 && string.Equals(existingStatus, "Flagged", StringComparison.OrdinalIgnoreCase))
        {
            reasons.Add($"Request flagged under {deptLabel} travel policy review thresholds.");
        }

        return (reasons.Count > 0, reasons);
    }
}
