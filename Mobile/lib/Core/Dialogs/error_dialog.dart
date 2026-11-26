import 'package:flutter/material.dart';
import 'package:lottie/lottie.dart';
import 'package:medlink/Core/Style/colors_manager.dart';
import 'package:medlink/Core/Style/text_styles.dart';

class ErrorDialog extends StatelessWidget {
  final String? error;
  const new({super.key, this.error});

  @override
  Widget build(BuildContext context) {
    return Dialog(
      backgroundColor: ColorsManager.primaryBackgroundColor,
      child: Padding(
        padding: const EdgeInsets.all(40.0),
        child: Column(
          mainAxisSize: .min,
          mainAxisAlignment: .center,
          spacing: 20,
          children: [
            Lottie.asset('assets/lotties/Error_Occurred!.json'),
            Text(
              error ?? '',
              style: TextStyles.authTitle.copyWith(fontSize: 13),
            ),
          ],
        ),
      ),
    );
  }
}
