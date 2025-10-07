namespace minuit2.net;

public class ParameterConfigurationException(string name, string message) : Exception($"{name}: {message}");