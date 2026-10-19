import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:flutter_svg/svg.dart';
import 'package:medlink/Core/Style/text_styles.dart';

class AuthScaffold extends StatelessWidget {
  final logo;
  final String title, titleDescription;
  final Widget child;
  const new({
    super.key,
    required this.logo,
    required this.title,
    required this.titleDescription,
    required this.child,
  });

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Column(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        crossAxisAlignment: .stretch,
        children: [
          Padding(
            padding: EdgeInsets.symmetric(vertical: 24.w, horizontal: 16),
            child: Column(
              children: [
                Column(
                  spacing: 20.w,
                  children: [
                    Column(
                      children: [
                        SizedBox(
                          width: 340.w,
                          height: 66.w,
                          child: Image.asset(logo),
                        ),
                        Text(
                          'Your Health, Our Connection.',
                          style: TextStyles.fadedText,
                        ),
                      ],
                    ),
                    Column(
                      children: [
                        Text(title, style: TextStyles.authTitle),
                        Text(titleDescription, style: TextStyles.fadedText),
                      ],
                    ),

                    child,
                  ],
                ),
              ],
            ),
          ),
          SvgPicture.asset(
            'assets/svgs/auth_svg.svg',
            height: 170.w,
            width: double.infinity,
          ),
        ],
      ),
    );
  }
}
