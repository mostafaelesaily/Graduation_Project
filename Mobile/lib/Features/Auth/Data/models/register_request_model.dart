import 'package:medlink/Features/Auth/Domain/Entites/register_request.dart';

class RegisterRequestModel extends RegisterRequest {
  RegisterRequestModel({
    required super.accountRole,
    required super.serviceType,
    required super.location,
    required super.name,
    required super.email,
    required super.phone,
    required super.password,
  });

  Map<String, dynamic> toJson() {
    return {
      'accountRole': accountRole,
      'serviceType': serviceType,
      'location': location,
      'name': name,
      'email': email,
      'phone': phone,
      'password': password,
    };
  }
}
