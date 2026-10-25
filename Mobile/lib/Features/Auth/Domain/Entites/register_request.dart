class RegisterRequest {
  final String accountRole;
  final String serviceType;
  final String location;
  final String name;
  final String email;
  final String phone;
  final String password;

  RegisterRequest({
    required this.accountRole,
    required this.serviceType,
    required this.location,
    required this.name,
    required this.email,
    required this.phone,
    required this.password,
  });
}
