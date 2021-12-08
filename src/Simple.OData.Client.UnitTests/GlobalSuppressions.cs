// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
	"Style",
	"IDE1006:Naming Styles",
	Justification = "Class is deliberately strangely capitalized for testing",
	Scope = "type",
	Target = "~T:Simple.OData.Client.Tests.FluentApi.FindTypedTests.OrderDetails")
]

[assembly: SuppressMessage(
	"Style",
	"IDE1006:Naming Styles",
	Justification = "Class is deliberately strangely capitalized for testing",
	Scope = "type",
	Target = "~T:Simple.OData.Client.Tests.FluentApi.FindTypedTests.orderDetails")
]

[assembly: SuppressMessage(
	"Style",
	"IDE0060:Remove unused parameter",
	Justification = "Unused parameter is present for testing",
	Scope = "type",
	Target = "~T:Simple.OData.Client.Tests.Extensions.DictionaryExtensionsTests.ClassNoDefaultConstructor")
]

[assembly: SuppressMessage(
	"Style",
	"IDE0150:Prefer 'null' check over type check",
	Justification = "Applying this breaks build",
	Scope = "member",
	Target = "~M:Simple.OData.Client.Tests.FluentApi.FindTypedTests.IsOfAssociation~System.Threading.Tasks.Task")
]
