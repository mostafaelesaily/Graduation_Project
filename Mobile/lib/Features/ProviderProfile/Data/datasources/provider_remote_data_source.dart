import 'dart:convert';

import 'package:medlink/Core/Network/api_client.dart';
import 'package:medlink/Features/ProviderProfile/Data/models/provider_profile_model.dart';

class ProviderRemoteDataSource {
  final ApiClient client;

  ProviderRemoteDataSource({required this.client});

  /// The API has no "list providers" endpoint, so this uses the unified search.
  /// The "%" keyword matches every service, around central Cairo.
  Future<List<ProviderProfileModel>> getProviders({int pageSize = 10}) async {
    const path = '/api/Search/UnifiedMedicalSearch';

    final response = await client.post(
      path,
      body: {
        'keyword': '%',
        'latitude': 30.0444,
        'longitude': 31.2357,
        'radiusInMeters': 50000,
        'pageNumber': 1,
        'pageSize': pageSize,
      },
    );

    if (response.statusCode != 200) {
      throw Exception('Loading providers failed: ${response.body}');
    }

    final decoded = jsonDecode(response.body);

    if (decoded['metadata']['internalSearchSucceeded'] == false) {
      throw Exception('Loading providers failed');
    }

    final items = decoded['results']['medlinkProviders']['items'] as List;

    return items.map((item) => ProviderProfileModel.fromJson(item)).toList();
  }

  Future<bool> isSlotAvailable(String providerId, DateTime slot) async {
    const path = '/api/Booking/availability';

    final response = await client.get(
      path,
      queryParameters: {
        'providerProfileId': providerId,
        'bookingDate': slot.toIso8601String(),
      },
    );

    if (response.statusCode != 200) {
      throw Exception('Availability check failed: ${response.body}');
    }

    return jsonDecode(response.body) == true;
  }

  Future<String> book(String providerId, DateTime slot) async {
    const path = '/api/Booking';

    final response = await client.post(
      path,
      body: {
        'providerProfileId': providerId,
        'bookingDate': slot.toIso8601String(),
      },
    );

    if (response.statusCode != 200 && response.statusCode != 201) {
      throw Exception('Booking failed: ${response.body}');
    }

    return jsonDecode(response.body)['id'];
  }
}
