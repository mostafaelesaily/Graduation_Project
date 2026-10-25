import 'dart:convert';

import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:medlink/Features/Auth/Domain/Repos/auth_repository.dart';
import 'package:medlink/Features/Auth/Presentation/Login/cubit/login_state.dart';

class LoginCubit extends Cubit<LoginState> {
  final AuthRepository authRepository;

  LoginCubit({required this.authRepository}) : super(const LoginState());

  Future<void> login({required String email, required String password}) async {
    if (state.status == LoginStatus.loading) return;

    emit(state.copyWith(status: LoginStatus.loading, clearError: true));

    try {
      await authRepository.login(email: email, password: password);
      emit(state.copyWith(status: LoginStatus.success, clearError: true));
    } on Exception catch (e) {
      emit(
        state.copyWith(
          status: LoginStatus.failure,
          errorMessage: _getErrorMessage(e),
        ),
      );
    }
  }

  Map<String, dynamic> _getErrorMessage(Object error) {
    final message = error.toString();

    try {
      final jsonStart = message.indexOf('{');
      final failureJson = message.substring(jsonStart);

      final notJson = message.substring(0, jsonStart);
      final errorSplit = notJson.split(':');
      final failed = errorSplit[1];

      final Map<String, dynamic> data = jsonDecode(failureJson);
      final error = {
        'failed': failed,
        'errorCode': data['StatusCode'],
        'message': data['Message'],
      };

      return error;
    } catch (_) {
      return {};
    }
  }

  void reset() {
    emit(const LoginState());
  }
}
