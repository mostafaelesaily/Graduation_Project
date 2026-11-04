import 'package:file_picker/file_picker.dart';

class RegisterRequest {
  final String accountRole;
  final PlatformFile? licenseFile;
  final String? serviceType;
  final String? serviceName;
  final String? serviceContactPhone;
  final String? postalCode;
  final String? taxNumber;
  final String? location;
  final String? nationalIdNumber;
  final String name;
  final String email;
  final String phone;
  final String password;

  RegisterRequest({
    required this.name,
    required this.email,
    required this.phone,
    required this.password,
    required this.accountRole,
    this.serviceType,
    this.serviceName,
    this.serviceContactPhone,
    this.postalCode,
    this.taxNumber,
    this.location,
    this.nationalIdNumber,
    this.licenseFile,
  });
}
