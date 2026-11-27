import 'dart:convert';

import 'package:medlink/Core/Network/api_client.dart';

class ProviderRemoteDataSource {
  final ApiClient client;

  ProviderRemoteDataSource({required this.client});

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

  Future<void> book(String providerId, DateTime slot) async {
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
  }
}
