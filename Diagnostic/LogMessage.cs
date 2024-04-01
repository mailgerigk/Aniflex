using System;

namespace Aniflex.Diagnostic;

public sealed record LogMessage(DateTime DateTime, string Message, bool IsException = false);
