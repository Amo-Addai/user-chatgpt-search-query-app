// This is a basic Flutter widget test.
//
// To perform an interaction with a widget in your test, use the WidgetTester
// utility in the flutter_test package. For example, you can send tap and scroll
// gestures. You can also use WidgetTester to find child widgets in the widget
// tree, read text, and verify that the values of widget properties are correct.

import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:mockito/mockito.dart';

import 'package:client/main.dart';

class MockClient extends Mock implements http.Client {}

void main() {
  group('Login page tests', () {
    testWidgets('Login page UI components', (WidgetTester tester) async {
      await tester.pumpWidget(MaterialApp(home: LoginPage()));

      expect(find.text('Login'), findsOneWidget);
      expect(find.byType(TextField), findsNWidgets(2));
      expect(find.byType(ElevatedButton), findsOneWidget);
    });

    testWidgets('Login with valid credentials', (WidgetTester tester) async {
      final client = MockClient();
      when(client.post(any, body: anyNamed('body')))
          .thenAnswer((_) async => http.Response('{"token": "test-token"}', 200));

      final loginPage = MaterialApp(home: LoginPage());
      await tester.pumpWidget(loginPage);

      await tester.enterText(find.byType(TextField).first, 'test_user');
      await tester.enterText(find.byType(TextField).last, 'test_password');
      await tester.tap(find.byType(ElevatedButton));
      await tester.pump();

      expect(find.byType(HomePage), findsOneWidget);
    });

    testWidgets('Login with invalid credentials', (WidgetTester tester) async {
      final client = MockClient();
      when(client.post(any, body: anyNamed('body')))
          .thenAnswer((_) async => http.Response('{"message": "Unauthorized"}', 401));

      final loginPage = MaterialApp(home: LoginPage());
      await tester.pumpWidget(loginPage);

      await tester.enterText(find.byType(TextField).first, 'invalid_user');
      await tester.enterText(find.byType(TextField).last, 'invalid_password');
      await tester.tap(find.byType(ElevatedButton));
      await tester.pump();

      expect(find.text('Authentication failed'), findsOneWidget);
    });
  });

  group('Home page tests', () {
    testWidgets('Home page UI components', (WidgetTester tester) async {
      await tester.pumpWidget(MaterialApp(home: HomePage()));

      expect(find.text('ChatGPT App'), findsOneWidget);
      expect(find.byIcon(Icons.logout), findsOneWidget);
    });
  });
}
