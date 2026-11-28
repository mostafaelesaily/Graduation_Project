import 'dart:convert';

import 'package:http/http.dart' as http;
import 'package:latlong2/latlong.dart';

/// The API does not return coordinates, so the address is looked up in
/// OpenStreetMap. The result is approximate.
class GeocodingDataSource {
  Future<LatLng?> findAddress(String address) async {
    final uri = Uri.https('nominatim.openstreetmap.org', '/search', {
      'q': address,
      'format': 'json',
      'limit': '1',
    });

    try {
      final response = await http
          .get(uri, headers: {'User-Agent': 'com.example.medlink'})
          .timeout(const Duration(seconds: 10));

      if (response.statusCode != 200) return null;

      final results = jsonDecode(response.body) as List;
      if (results.isEmpty) return null;

      return LatLng(
        double.parse(results.first['lat']),
        double.parse(results.first['lon']),
      );
    } catch (_) {
      return null;
    }
  }
}
