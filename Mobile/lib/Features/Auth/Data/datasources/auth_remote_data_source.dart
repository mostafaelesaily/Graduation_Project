import 'dart:convert';

import 'package:medlink/Core/Network/api_client.dart';
import 'package:medlink/Features/Auth/Data/models/auth_tokens_model.dart';
import 'package:medlink/Features/Auth/Data/models/register_request_model.dart';

class AuthRemoteDataSource {
  final ApiClient client;

  AuthRemoteDataSource({required this.client});

  Future<AuthTokensModel> login({
    required String email,
    required String password,
  }) async {
    const path = '/api/Account/Login';

    final response = await client.post(
      path,
      body: {'email': email, 'password': password},
      requiresAuth: false,
    );

    if (response.statusCode != 200 && response.statusCode != 201) {
      throw Exception('Login failed: ${response.body}');
    }

    final decoded = jsonDecode(response.body);

    if (decoded is! Map<String, dynamic>) {
      throw const FormatException('Invalid login response format');
    }

    return AuthTokensModel.fromJson(decoded);
  }

  Future<String> register(RegisterRequestModel model) async {
    const path = '/api/Account/Register';

    final response = await client.post(
      path,
      body: model.toJson(),
      requiresAuth: false,
    );

    if (response.statusCode != 200 && response.statusCode != 201) {
      throw Exception('Registration failed: ${response.body}');
    }

    return jsonDecode(response.body)['message'];
  }

  Future<void> logout({required String refreshToken}) async {
    const path = '/api/Account/Logout';

    final response = await client.post(path);

    if (response.statusCode != 200 && response.statusCode != 204) {
      throw Exception('Logout failed: ${response.body}');
    }
  }
}
