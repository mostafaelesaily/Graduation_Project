import 'package:medlink/Features/Auth/Domain/Entites/register_request.dart';
import 'package:medlink/Features/Auth/Presentation/Signup/View/signup_screen.dart';

class RegisterRequestModel extends RegisterRequest {
  RegisterRequestModel({
    required super.name,
    required super.email,
    required super.phone,
    required super.password,
    required super.accountRole,
    super.serviceType,
    super.serviceName,
    super.serviceContactPhone,
    super.postalCode,
    super.taxNumber,
    super.location,
    super.nationalIdNumber,
    super.licenseFile,
  });

  Map<String, dynamic> toJson() {
    return {
      if (accountRole == AccountRole.provider.name) ...{
        'serviceType': serviceType,
        'location': location,
        'serviceName': serviceName,
        'serviceContactPhone': serviceContactPhone,
        'postalCode': postalCode,
        'taxNumber': taxNumber,
        'nationalIdNumber': nationalIdNumber,
      },
      'name': name,
      'email': email,
      'phone': phone,
      'password': password,
    };
  }
}
