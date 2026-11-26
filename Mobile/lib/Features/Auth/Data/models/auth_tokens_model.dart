import 'package:medlink/Features/Auth/Domain/Entites/auth_tokens.dart';

class AuthTokensModel extends AuthTokens {
  AuthTokensModel({required super.accessToken, required super.refreshToken});

  factory AuthTokensModel.fromJson(Map<String, dynamic> json) {
    return AuthTokensModel(
      accessToken: json['access_token'],
      refreshToken: json['refresh_token'],
    );
  }
}
