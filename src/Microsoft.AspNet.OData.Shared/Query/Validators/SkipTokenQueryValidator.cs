﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Query.Validators
{
    /// <summary>
    /// Represents a validator used to validate a <see cref="SkipTokenQueryOption"/> based on the <see cref="ODataValidationSettings"/>.
    /// </summary>
    public class SkipTokenQueryValidator
    {
        /// <summary>
        /// Validates a <see cref="SkipTokenQueryOption" />.
        /// </summary>
        /// <param name="skipToken">The $skip query.</param>
        /// <param name="validationSettings">The validation settings.</param>
        public virtual void Validate(SkipTokenQueryOption skipToken, ODataValidationSettings validationSettings)
        {
            if (skipToken == null)
            {
                throw Error.ArgumentNull("skipQueryOption");
            }

            if (validationSettings == null)
            {
                throw Error.ArgumentNull("validationSettings");
            }
            //See what additional checks need to be in place given the validation settings
        }

        internal static SkipTokenQueryValidator GetSkipTokenQueryValidator(ODataQueryContext context)
        {
            if (context == null || context.RequestContainer == null)
            {
                return new SkipTokenQueryValidator();
            }
            return context.RequestContainer.GetRequiredService<SkipTokenQueryValidator>();
        }
    }
}
