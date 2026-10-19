import 'package:flutter/material.dart';
import 'package:medlink/Core/Shared/input_field.dart';
import 'package:medlink/Core/Widgets/auth_scaffold.dart';

class LoginScreen extends StatefulWidget {
  new({super.key});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  late final GlobalKey<FormState> _formKey;
  late final TextEditingController _controller;

  @override
  void initState() {
    super.initState();
    _formKey = GlobalKey<FormState>();
    _controller = TextEditingController();
  }

  @override
  Widget build(BuildContext context) {
    return AuthScaffold(
      logo: 'assets/logo/logo.png',
      title: 'Create your Account',
      titleDescription: 'Choose your role to get started',
      child: Form(
        key: _formKey,
        child: Column(
          children: [
            InputField(
              controller: _controller,
              validator: (value) {},
              isRequired: true,
              labelText: 'Full Name',
              hintText: 'Enter your full name',
              icon: Icons.person,
            ),
          ],
        ),
      ),
    );
  }
}
