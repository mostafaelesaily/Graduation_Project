import 'dart:convert';

import 'package:http/http.dart';
import 'package:medlink/Core/Network/token_storage.dart';

class ApiClient {
  final String _baseUrl;
  final Client _client;
  final TokenStorage tokenStorage;

  ApiClient({
    required String baseUrl,
    required this.tokenStorage,
    Client? client,
  }) : _baseUrl = baseUrl,
       _client = client ?? Client();

  String? accessToken;
  Future<bool>? _refreshFuture;

  Uri _buildUri(String path, [Map<String, dynamic>? queryParameters]) {
    final uri = Uri.parse(_baseUrl).resolve(path);

    if (queryParameters == null || queryParameters.isEmpty) {
      return uri;
    }

    return uri.replace(
      queryParameters: {...uri.queryParameters, ...queryParameters},
    );
  }

  Future<Response> _request(
    String method,
    String path, {
    Object? body,
    Map<String, dynamic>? queryParameters,
    Map<String, dynamic>? headers,
    bool requiresAuth = true,
    bool retryAfterRefresh = true,
  }) async {
    final uri = _buildUri(path, queryParameters);
    final requestHeaders = <String, String>{
      'Accept': 'application/json',
      if (body != null) 'Content-Type': 'application/json',
      ...?headers,
    };

    if (requiresAuth) {
      final accessToken = await tokenStorage.getAccessToken();
      if (accessToken != null && accessToken.isNotEmpty) {
        requestHeaders['Authorization'] = 'Bearer $accessToken';
      }
    }

    final request = Request(method, uri)..headers.addAll(requestHeaders);

    if (body != null) {
      request.body = jsonEncode(body);
    }

    final streamedResponse = await _client.send(request);
    final response = await Response.fromStream(streamedResponse);

    if (response.statusCode != 401 || !requiresAuth || !retryAfterRefresh) {
      return response;
    }

    final failedAccessToken = requestHeaders['Authorization'];

    final refreshed = await _refreshOnce(failedAccessToken);

    if (!refreshed) {
      await tokenStorage.clearTokens();
      return response;
    }

    return _request(
      method,
      path,
      body: body,
      queryParameters: queryParameters,
      headers: headers,
      requiresAuth: requiresAuth,
      retryAfterRefresh: false,
    );
  }

  Future<bool> _refreshOnce(String? failedAuthorization) async {
    final currentToken = await tokenStorage.getAccessToken();

    if (currentToken != null &&
        currentToken.isNotEmpty &&
        'Bearer $currentToken' != failedAuthorization) {
      return true;
    }

    final existingRefresh = _refreshFuture;
    if (existingRefresh != null) {
      return existingRefresh;
    }

    final refreshFuture = refreshTokens();
    _refreshFuture = refreshFuture;

    try {
      return await refreshFuture;
    } finally {
      if (identical(_refreshFuture, refreshFuture)) {
        _refreshFuture = null;
      }
    }
  }

  Future<bool> refreshTokens() async {
    try {
      final refreshToken = await tokenStorage.getRefreshToken();

      if (refreshToken == null || refreshToken.isEmpty) {
        return false;
      }

      final response = await _client.post(
        _buildUri('/api/Account/HandleRefreshToken'),
        headers: {
          'Accept': 'application/json',
          'Content-Type': 'application/json',
        },
        body: jsonEncode({'refreshToken': refreshToken}),
      );

      if (response.statusCode < 200 || response.statusCode >= 300) {
        return false;
      }

      if (response.body.isEmpty) {
        return false;
      }

      final decoded = jsonDecode(response.body);

      if (decoded is! Map<String, dynamic>) {
        return false;
      }

      final newAccessToken = decoded['access_token'];
      final newRefreshToken = decoded['refresh_token'];

      if (newAccessToken is! String ||
          newAccessToken.isEmpty ||
          newRefreshToken is! String ||
          newRefreshToken.isEmpty) {
        return false;
      }

      await tokenStorage.saveTokens(
        accessToken: newAccessToken,
        refreshToken: newRefreshToken,
      );

      return true;
    } catch (_) {
      return false;
    }
  }

  Future<Response> get(
    String path, {
    Map<String, String>? queryParameters,
    Map<String, String>? headers,
    bool requiresAuth = true,
  }) {
    return _request(
      'GET',
      path,
      queryParameters: queryParameters,
      headers: headers,
      requiresAuth: requiresAuth,
    );
  }

  Future<Response> post(
    String path, {
    Object? body,
    Map<String, String>? queryParameters,
    Map<String, String>? headers,
    bool requiresAuth = true,
  }) {
    return _request(
      'POST',
      path,
      body: body,
      queryParameters: queryParameters,
      headers: headers,
      requiresAuth: requiresAuth,
    );
  }

  Future<Response> put(
    String path, {
    Object? body,
    Map<String, String>? queryParameters,
    Map<String, String>? headers,
    bool requiresAuth = true,
  }) {
    return _request(
      'PUT',
      path,
      body: body,
      queryParameters: queryParameters,
      headers: headers,
      requiresAuth: requiresAuth,
    );
  }

  Future<Response> delete(
    String path, {
    Object? body,
    Map<String, String>? queryParameters,
    Map<String, String>? headers,
    bool requiresAuth = true,
  }) {
    return _request(
      'DELETE',
      path,
      body: body,
      queryParameters: queryParameters,
      headers: headers,
      requiresAuth: requiresAuth,
    );
  }
}
