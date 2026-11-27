import 'package:medlink/Features/ProviderProfile/Domain/Entites/provider_profile.dart';

/// One item of results.medlinkProviders.items in the unified medical search.
class ProviderProfileModel extends ProviderProfile {
  const ProviderProfileModel({
    required super.id,
    required super.serviceName,
    required super.address,
    required super.phoneNumber,
  });

  factory ProviderProfileModel.fromJson(Map<String, dynamic> json) {
    return ProviderProfileModel(
      id: json['providerId'],
      serviceName: json['serviceName'] ?? '',
      address: json['address'] ?? '',
      phoneNumber: json['phoneNumber'] ?? '',
    );
  }
}
