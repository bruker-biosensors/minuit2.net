namespace minuit2.net.Exceptions;

public class ParameterConfigurationException(string name, string message) : Exception($"{name}: {message}");